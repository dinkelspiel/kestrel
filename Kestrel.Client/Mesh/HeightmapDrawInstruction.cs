using System.Numerics;
using Kestrel.Client.MMath;
using Kestrel.Client.Renderer;
using Silk.NET.OpenGL;

namespace Kestrel.Client.Mesh;

public class HeightmapDrawInstruction(ClientContext clientContext, Vector2 tileSize, Matrix4x4 translation, (int X, int Y) atlasPosition) : IDrawInstruction
{
    public static uint Vao, Vbo, Ebo;

    static float[] Vertices =
    [

    ];


    static uint[] Indices =
    [
    ];

    public static float[,] Heightmap = null!;
    public static int Size;

    public static float SampleHeight(float[,] heightmap, int size, float x, float y)
    {
        if (size <= 0)
            return 0f;

        x = Math.Clamp(x, 0f, size - 1f);
        y = Math.Clamp(y, 0f, size - 1f);

        int x0 = (int)MathF.Floor(x);
        int y0 = (int)MathF.Floor(y);
        int x1 = Math.Min(x0 + 1, size - 1);
        int y1 = Math.Min(y0 + 1, size - 1);
        float tx = x - x0;
        float ty = y - y0;

        float h00 = heightmap[x0, y0];
        float h10 = heightmap[x1, y0];
        float h01 = heightmap[x0, y1];
        float h11 = heightmap[x1, y1];
        float hx0 = h00 + (h10 - h00) * tx;
        float hx1 = h01 + (h11 - h01) * tx;
        return hx0 + (hx1 - hx0) * ty;
    }

    public static unsafe void Setup(ClientContext clientContext)
    {
        FastNoiseLite noise = new();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        Size = 712;
        float center = Size * 0.5f;
        float radius = Size * 0.4f;

        Heightmap = new float[Size, Size];

        float GetHeight(int x, int y)
        {
            float distanceToCenter = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
            float falloff = Math.Clamp(1f - distanceToCenter / radius, -0.5f, 1f);
            return (noise.GetNoise(x, y) + 1) * 24f * falloff;
        }


        for (int x = 0; x < Size - 1; x++)
        {
            for (int y = 0; y < Size - 1; y++)
            {
                Heightmap[x, y] = GetHeight(x, y);
            }
        }

        Vector3 GetNormal(int x, int y)
        {
            float left = Heightmap[Math.Max(x - 1, 0), y];
            float right = Heightmap[Math.Min(x + 1, Size - 1), y];
            float down = Heightmap[x, Math.Max(y - 1, 0)];
            float up = Heightmap[x, Math.Min(y + 1, Size - 1)];

            return Vector3.Normalize(new Vector3(left - right, 2f, down - up));
        }

        List<float> verticies = [];
        List<uint> indicies = [];

        void AddVertex(int x, int y, float height)
        {
            Vector3 normal = GetNormal(x, y);
            verticies.AddRange([x, height, y, 0.5f, 0.5f, normal.X, normal.Y, normal.Z]);
        }

        for (int x = 0; x < Size - 1; x++)
        {
            for (int y = 0; y < Size - 1; y++)
            {
                float h00 = Heightmap[x, y];
                float h01 = Heightmap[x, y + 1];
                float h10 = Heightmap[x + 1, y];
                float h11 = Heightmap[x + 1, y + 1];
                uint i = (uint)(verticies.Count / 8);

                AddVertex(x, y, h00);
                AddVertex(x, y + 1, h01);
                AddVertex(x + 1, y, h10);
                AddVertex(x + 1, y + 1, h11);

                indicies.AddRange([i, i + 1, i + 2]);
                indicies.AddRange([i + 1, i + 3, i + 2]);
            }
        }

        Vertices = [.. verticies];
        Indices = [.. indicies];


        Vao = clientContext.Gl.GenVertexArray();
        clientContext.Gl.BindVertexArray(Vao);

        Vbo = clientContext.Gl.GenBuffer();
        clientContext.Gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo);
        fixed (float* v = Vertices)
            clientContext.Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(Vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);

        Ebo = clientContext.Gl.GenBuffer();
        clientContext.Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Ebo);
        fixed (uint* i = Indices)
            clientContext.Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);

        const uint stride = 8 * sizeof(float);
        clientContext.Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
        clientContext.Gl.EnableVertexAttribArray(0);
        clientContext.Gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
        clientContext.Gl.EnableVertexAttribArray(1);
        clientContext.Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, (void*)(5 * sizeof(float)));
        clientContext.Gl.EnableVertexAttribArray(2);
    }

    public unsafe void Draw(Matrix4x4 view, Matrix4x4 projection, Renderer.Shader shader)
    {
        shader.SetMatrix4("uView", view);
        shader.SetMatrix4("uProjection", projection);
        clientContext.Gl.BindVertexArray(Vao);

        shader.SetVector2("uTileOffset", new Vector2(tileSize.X * atlasPosition.X, tileSize.Y * atlasPosition.Y));
        shader.SetVector2("uTileSize", tileSize);
        shader.SetInt("uIsHeightmap", 1);
        shader.SetInt("uIsGrass", 0);
        shader.SetMatrix4("uModel", translation);
        clientContext.Gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
    }

    public static void CleanUp(ClientContext clientContext)
    {
        clientContext.Gl.DeleteVertexArray(Vao);
        clientContext.Gl.DeleteBuffer(Vbo);
        clientContext.Gl.DeleteBuffer(Ebo);
    }

    public ShaderKind GetShader() => ShaderKind.REGULAR;
}