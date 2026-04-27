using System.Numerics;
using Silk.NET.OpenGL;

namespace Kestrel.Client.Mesh;

public class CubeDrawInstruction(ClientContext clientContext, Vector2 tileSize, Matrix4x4 translation, (int X, int Y) atlasPosition) : IDrawInstruction
{
    public static uint Vao, Vbo, Ebo;

    static readonly float[] Vertices =
    [
        // front (+Z)
        -0.5f, -0.5f,  0.5f,  0f, 1f,
         0.5f, -0.5f,  0.5f,  1f, 1f,
         0.5f,  0.5f,  0.5f,  1f, 0f,
        -0.5f,  0.5f,  0.5f,  0f, 0f,
        // back (-Z)
         0.5f, -0.5f, -0.5f,  0f, 1f,
        -0.5f, -0.5f, -0.5f,  1f, 1f,
        -0.5f,  0.5f, -0.5f,  1f, 0f,
         0.5f,  0.5f, -0.5f,  0f, 0f,
        // right (+X)
         0.5f, -0.5f,  0.5f,  0f, 1f,
         0.5f, -0.5f, -0.5f,  1f, 1f,
         0.5f,  0.5f, -0.5f,  1f, 0f,
         0.5f,  0.5f,  0.5f,  0f, 0f,
        // left (-X)
        -0.5f, -0.5f, -0.5f,  0f, 1f,
        -0.5f, -0.5f,  0.5f,  1f, 1f,
        -0.5f,  0.5f,  0.5f,  1f, 0f,
        -0.5f,  0.5f, -0.5f,  0f, 0f,
        // top (+Y)
        -0.5f,  0.5f,  0.5f,  0f, 1f,
         0.5f,  0.5f,  0.5f,  1f, 1f,
         0.5f,  0.5f, -0.5f,  1f, 0f,
        -0.5f,  0.5f, -0.5f,  0f, 0f,
        // bottom (-Y)
        -0.5f, -0.5f, -0.5f,  0f, 1f,
         0.5f, -0.5f, -0.5f,  1f, 1f,
         0.5f, -0.5f,  0.5f,  1f, 0f,
        -0.5f, -0.5f,  0.5f,  0f, 0f,
    ];


    static readonly uint[] Indices =
    [
         0,  1,  2,   2,  3,  0,
         4,  5,  6,   6,  7,  4,
         8,  9, 10,  10, 11,  8,
        12, 13, 14,  14, 15, 12,
        16, 17, 18,  18, 19, 16,
        20, 21, 22,  22, 23, 20,
    ];

    public static unsafe void Setup(ClientContext clientContext)
    {
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
        shader.SetInt("uIsHeightmap", 0);
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