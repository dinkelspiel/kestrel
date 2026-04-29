using System.Globalization;
using System.Numerics;
using Kestrel.Client.Renderer;
using Silk.NET.OpenGL;

namespace Kestrel.Client.Mesh;

public class ModelDrawInstruction : IDrawInstruction, IDisposable
{
    readonly ClientContext clientContext;
    readonly Renderer.Texture texture;
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
        texture = texturePath is null
            ? new Renderer.Texture(clientContext.Gl, [255, 255, 255, 255], 1, 1)
            : new Renderer.Texture(clientContext.Gl, texturePath);

        LoadObj(objPath, out float[] vertices, out uint[] indices);
        indexCount = (uint)indices.Length;

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

        texture.Bind(TextureUnit.Texture0);
        clientContext.Gl.BindVertexArray(vao);
        clientContext.Gl.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, null);
    }

    public ShaderKind GetShader() => ShaderKind.REGULAR;

    public void Dispose()
    {
        clientContext.Gl.DeleteVertexArray(vao);
        clientContext.Gl.DeleteBuffer(vbo);
        clientContext.Gl.DeleteBuffer(ebo);
        texture.Dispose();
        GC.SuppressFinalize(this);
    }

    public void CleanUp() => Dispose();

    static void LoadObj(string path, out float[] vertices, out uint[] indices)
    {
        List<Vector3> positions = [];
        List<Vector2> texCoords = [];
        List<float> vertexData = [];
        List<uint> indexData = [];
        Dictionary<string, uint> vertexLookup = [];

        foreach (string rawLine in File.ReadLines(path))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line[0] == '#')
                continue;

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0])
            {
                case "v":
                    positions.Add(new Vector3(ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3])));
                    break;
                case "vt":
                    texCoords.Add(new Vector2(ParseFloat(parts[1]), ParseFloat(parts[2])));
                    break;
                case "f":
                    AddFace(parts, positions, texCoords, vertexLookup, vertexData, indexData);
                    break;
            }
        }

        vertices = [.. vertexData];
        indices = [.. indexData];
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
            texCoord = texCoords[texCoordIndex];
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
}
