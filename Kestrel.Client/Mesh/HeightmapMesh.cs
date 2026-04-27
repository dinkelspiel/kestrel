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
        int size = 512;
        float center = size * 0.5f;
        float radius = size * 0.4f;

        float GetHeight(int x, int y)
        {
            float distanceToCenter = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
            float falloff = Math.Clamp(1f - distanceToCenter / radius, -0.5f, 1f);
            return (noise.GetNoise(x, y) + 1) * 24f * falloff;
        }

        List<float> verticies = [];
        List<uint> indicies = [];

        for (int x = 0; x < size - 1; x++)
        {
            for (int y = 0; y < size - 1; y++)
            {
                float h00 = GetHeight(x, y);
                float h01 = GetHeight(x, y + 1);
                float h10 = GetHeight(x + 1, y);
                float h11 = GetHeight(x + 1, y + 1);
                uint i = (uint)(verticies.Count / 5);

                verticies.AddRange([x, h00, y, 0.5f, 0.5f]);
                verticies.AddRange([x, h01, y + 1, 0.5f, 0.5f]);
                verticies.AddRange([x + 1, h10, y, 0.5f, 0.5f]);
                verticies.AddRange([x + 1, h11, y + 1, 0.5f, 0.5f]);

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