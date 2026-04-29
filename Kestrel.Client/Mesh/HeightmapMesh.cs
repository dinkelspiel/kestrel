using System.Numerics;
using Kestrel.Client.MMath;
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

    public static unsafe void Setup(ClientContext clientContext)
    {
        FastNoiseLite noise = new();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        int size = 712;
        float center = size * 0.5f;
        float radius = size * 0.3f;

        float GetHeight(int x, int y)
        {
            float distanceToCenter = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
            float falloff = Math.Clamp(1f - distanceToCenter / radius, -0.5f, 1f);
            return (noise.GetNoise(x, y) + 1) * 24f * falloff;
        }

        Vector3 GetNormal(int x, int y)
        {
            float left = GetHeight(Math.Max(x - 1, 0), y);
            float right = GetHeight(Math.Min(x + 1, size - 1), y);
            float down = GetHeight(x, Math.Max(y - 1, 0));
            float up = GetHeight(x, Math.Min(y + 1, size - 1));

            return Vector3.Normalize(new Vector3(left - right, 2f, down - up));
        }

        List<float> verticies = [];
        List<uint> indicies = [];

        void AddVertex(int x, int y, float height)
        {
            Vector3 normal = GetNormal(x, y);
            verticies.AddRange([x, height, y, 0.5f, 0.5f, normal.X, normal.Y, normal.Z]);
        }

        for (int x = 0; x < size - 1; x++)
        {
            for (int y = 0; y < size - 1; y++)
            {
                float h00 = GetHeight(x, y);
                float h01 = GetHeight(x, y + 1);
                float h10 = GetHeight(x + 1, y);
                float h11 = GetHeight(x + 1, y + 1);
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
        shader.SetMatrix4("uModel", translation);
        clientContext.Gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
    }

    public static void CleanUp(ClientContext clientContext)
    {
        clientContext.Gl.DeleteVertexArray(Vao);
        clientContext.Gl.DeleteBuffer(Vbo);
        clientContext.Gl.DeleteBuffer(Ebo);
    }
}