using System.Globalization;
using System.Numerics;
using Kestrel.Client.Renderer;
using Silk.NET.OpenGL;

namespace Kestrel.Client.Mesh;

public class ModelDrawInstruction : IDrawInstruction, IDisposable
{
    readonly ClientContext clientContext;
    readonly List<ModelPart> modelParts = [];
    readonly List<Renderer.Texture> textures = [];
    readonly uint vao;
    readonly uint vbo;
    readonly uint ebo;
    readonly uint indexCount;

    public Matrix4x4 Transform;

    public ModelDrawInstruction(ClientContext clientContext, string objPath)
        : this(clientContext, objPath, null, Matrix4x4.Identity)
    {
    }

    public ModelDrawInstruction(ClientContext clientContext, string objPath, string texturePath)
        : this(clientContext, objPath, texturePath, Matrix4x4.Identity)
    {
    }

    public unsafe ModelDrawInstruction(ClientContext clientContext, string objPath, string? texturePath, Matrix4x4 transform)
    {
        this.clientContext = clientContext;
        Transform = transform;

        LoadObj(objPath, out float[] vertices, out uint[] indices, out List<ObjPart> objParts, out Dictionary<string, string> materialTextures);
        indexCount = (uint)indices.Length;
        LoadTextures(texturePath, objParts, materialTextures);

        vao = clientContext.Gl.GenVertexArray();
        clientContext.Gl.BindVertexArray(vao);

        vbo = clientContext.Gl.GenBuffer();
        clientContext.Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        fixed (float* v = vertices)
            clientContext.Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);

        ebo = clientContext.Gl.GenBuffer();
        clientContext.Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        fixed (uint* i = indices)
            clientContext.Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);

        const uint stride = 5 * sizeof(float);
        clientContext.Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
        clientContext.Gl.EnableVertexAttribArray(0);
        clientContext.Gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
        clientContext.Gl.EnableVertexAttribArray(1);
    }

    public unsafe void Draw(Matrix4x4 view, Matrix4x4 projection, Renderer.Shader shader)
    {
        shader.SetMatrix4("uView", view);
        shader.SetMatrix4("uProjection", projection);
        shader.SetVector2("uTileOffset", Vector2.Zero);
        shader.SetVector2("uTileSize", Vector2.One);
        shader.SetInt("uIsHeightmap", 0);
        shader.SetInt("uIsGrass", 0);
        shader.SetMatrix4("uModel", Transform);

        clientContext.Gl.BindVertexArray(vao);
        foreach (var part in modelParts)
        {
            bool cullFaceEnabled = clientContext.Gl.IsEnabled(EnableCap.CullFace);
            if (part.TwoSided && cullFaceEnabled)
                clientContext.Gl.Disable(EnableCap.CullFace);

            part.Texture.Bind(TextureUnit.Texture0);
            clientContext.Gl.DrawElements(PrimitiveType.Triangles, part.IndexCount, DrawElementsType.UnsignedInt, (void*)(part.IndexStart * sizeof(uint)));

            if (part.TwoSided && cullFaceEnabled)
                clientContext.Gl.Enable(EnableCap.CullFace);
        }
    }

    public ShaderKind GetShader() => ShaderKind.REGULAR;

    public void Dispose()
    {
        clientContext.Gl.DeleteVertexArray(vao);
        clientContext.Gl.DeleteBuffer(vbo);
        clientContext.Gl.DeleteBuffer(ebo);
        foreach (var texture in textures)
            texture.Dispose();
        GC.SuppressFinalize(this);
    }

    public void CleanUp() => Dispose();

    void LoadTextures(string? texturePath, List<ObjPart> objParts, Dictionary<string, string> materialTextures)
    {
        if (texturePath is not null)
        {
            var texture = new Renderer.Texture(clientContext.Gl, texturePath);
            textures.Add(texture);
            modelParts.Add(new ModelPart(texture, 0, indexCount));
            return;
        }

        var whiteTexture = new Renderer.Texture(clientContext.Gl, [255, 255, 255, 255], 1, 1);
        textures.Add(whiteTexture);
        Dictionary<string, Renderer.Texture> textureByPath = [];

        foreach (var objPart in objParts)
        {
            var texture = whiteTexture;
            string? materialName = objPart.MaterialName;
            bool twoSided = IsTwoSidedMaterial(materialName, null);
            if (materialName is not null && materialTextures.TryGetValue(materialName, out var materialTexturePath))
            {
                twoSided = IsTwoSidedMaterial(materialName, materialTexturePath);
                if (textureByPath.TryGetValue(materialTexturePath, out var existingTexture))
                {
                    texture = existingTexture;
                }
                else
                {
                    texture = new Renderer.Texture(clientContext.Gl, materialTexturePath);
                    textureByPath[materialTexturePath] = texture;
                    textures.Add(texture);
                }
            }

            modelParts.Add(new ModelPart(texture, objPart.IndexStart, objPart.IndexCount, twoSided));
        }
    }

    static bool IsTwoSidedMaterial(string? materialName, string? texturePath)
    {
        return materialName?.Contains("leaf", StringComparison.OrdinalIgnoreCase) == true
            || Path.GetFileNameWithoutExtension(texturePath)?.Contains("leaf", StringComparison.OrdinalIgnoreCase) == true;
    }

    static void LoadObj(string path, out float[] vertices, out uint[] indices, out List<ObjPart> objParts, out Dictionary<string, string> materialTextures)
    {
        List<Vector3> positions = [];
        List<Vector2> texCoords = [];
        List<float> vertexData = [];
        List<uint> indexData = [];
        Dictionary<string, uint> vertexLookup = [];
        List<ObjPart> partsByMaterial = [];
        Dictionary<string, string> textureByMaterial = [];
        string? currentMaterial = null;
        int currentPartStart = 0;
        bool hasFacesInPart = false;

        foreach (string rawLine in File.ReadLines(path))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line[0] == '#')
                continue;

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0])
            {
                case "mtllib":
                    LoadMtl(path, parts, textureByMaterial);
                    break;
                case "usemtl":
                    AddObjPart(partsByMaterial, currentMaterial, currentPartStart, indexData.Count, hasFacesInPart);
                    currentMaterial = parts.Length > 1 ? parts[1] : null;
                    currentPartStart = indexData.Count;
                    hasFacesInPart = false;
                    break;
                case "v":
                    positions.Add(new Vector3(ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3])));
                    break;
                case "vt":
                    texCoords.Add(new Vector2(ParseFloat(parts[1]), ParseFloat(parts[2])));
                    break;
                case "f":
                    AddFace(parts, positions, texCoords, vertexLookup, vertexData, indexData);
                    hasFacesInPart = true;
                    break;
            }
        }

        AddObjPart(partsByMaterial, currentMaterial, currentPartStart, indexData.Count, hasFacesInPart);

        vertices = [.. vertexData];
        indices = [.. indexData];
        objParts = partsByMaterial.Count > 0 ? partsByMaterial : [new ObjPart(null, 0, (uint)indexData.Count)];
        materialTextures = textureByMaterial;
    }

    static void AddObjPart(List<ObjPart> objParts, string? materialName, int start, int end, bool hasFaces)
    {
        if (hasFaces && end > start)
            objParts.Add(new ObjPart(materialName, start, (uint)(end - start)));
    }

    static void LoadMtl(string objPath, string[] parts, Dictionary<string, string> textureByMaterial)
    {
        string objDirectory = Path.GetDirectoryName(objPath) ?? string.Empty;
        foreach (string mtlFile in parts.Skip(1))
        {
            string mtlPath = Path.Combine(objDirectory, mtlFile);
            if (!File.Exists(mtlPath))
                continue;

            string? currentMaterial = null;
            string mtlDirectory = Path.GetDirectoryName(mtlPath) ?? string.Empty;
            foreach (string rawLine in File.ReadLines(mtlPath))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line[0] == '#')
                    continue;

                string[] mtlParts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                switch (mtlParts[0])
                {
                    case "newmtl":
                        currentMaterial = mtlParts.Length > 1 ? mtlParts[1] : null;
                        break;
                    case "map_Kd" when currentMaterial is not null && mtlParts.Length > 1:
                        textureByMaterial[currentMaterial] = Path.Combine(mtlDirectory, mtlParts[1]);
                        break;
                }
            }
        }
    }

    static void AddFace(string[] parts, List<Vector3> positions, List<Vector2> texCoords, Dictionary<string, uint> vertexLookup, List<float> vertexData, List<uint> indexData)
    {
        if (parts.Length < 4)
            return;

        uint[] faceIndices = new uint[parts.Length - 1];
        for (int i = 1; i < parts.Length; i++)
            faceIndices[i - 1] = GetVertexIndex(parts[i], positions, texCoords, vertexLookup, vertexData);

        for (int i = 1; i < faceIndices.Length - 1; i++)
        {
            indexData.Add(faceIndices[0]);
            indexData.Add(faceIndices[i]);
            indexData.Add(faceIndices[i + 1]);
        }
    }

    static uint GetVertexIndex(string token, List<Vector3> positions, List<Vector2> texCoords, Dictionary<string, uint> vertexLookup, List<float> vertexData)
    {
        if (vertexLookup.TryGetValue(token, out uint existingIndex))
            return existingIndex;

        string[] parts = token.Split('/');
        int positionIndex = ResolveIndex(parts[0], positions.Count);
        Vector3 position = positions[positionIndex];
        Vector2 texCoord = Vector2.Zero;

        if (parts.Length > 1 && parts[1].Length > 0)
        {
            int texCoordIndex = ResolveIndex(parts[1], texCoords.Count);
            texCoord = new Vector2(texCoords[texCoordIndex].X, 1f - texCoords[texCoordIndex].Y);
        }

        uint index = (uint)(vertexData.Count / 5);
        vertexData.AddRange([position.X, position.Y, position.Z, texCoord.X, texCoord.Y]);
        vertexLookup[token] = index;
        return index;
    }

    static int ResolveIndex(string value, int count)
    {
        int index = int.Parse(value, CultureInfo.InvariantCulture);
        return index > 0 ? index - 1 : count + index;
    }

    static float ParseFloat(string value) => float.Parse(value, CultureInfo.InvariantCulture);

    readonly record struct ModelPart(Renderer.Texture Texture, int IndexStart, uint IndexCount, bool TwoSided = false);
    readonly record struct ObjPart(string? MaterialName, int IndexStart, uint IndexCount);
}
