using System.Numerics;
using Silk.NET.OpenGL;

namespace Kestrel.Client.Mesh;

public class BillboardDrawInstruction(ClientContext clientContext, Vector2 tileSize, Matrix4x4 translation, (int X, int Y) atlasPosition) : IDrawInstruction
{
    public static uint BillboardVao, BillboardVbo, BillboardEbo;

    static readonly float[] BillboardVertices =
    [
        -0.5f*16, -0.5f * 16,  0f*16,  0f, 1f,
         0.5f*16, -0.5f * 16,  0f*16,  1f, 1f,
         0.5f*16,  0.5f * 16,  0f*16,  1f, 0f,
        -0.5f*16,  0.5f * 16,  0f*16,  0f, 0f,
    ];

    static readonly uint[] BillboardIndices =
    [
         2,  1,  0,   0,  3,  2,
    ];

    public static unsafe void Setup(ClientContext clientContext)
    {
        BillboardVao = clientContext.Gl.GenVertexArray();
        clientContext.Gl.BindVertexArray(BillboardVao);

        BillboardVbo = clientContext.Gl.GenBuffer();
        clientContext.Gl.BindBuffer(BufferTargetARB.ArrayBuffer, BillboardVbo);
        fixed (float* v = BillboardVertices)
            clientContext.Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(BillboardVertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);

        BillboardEbo = clientContext.Gl.GenBuffer();
        clientContext.Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, BillboardEbo);
        fixed (uint* i = BillboardIndices)
            clientContext.Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(BillboardIndices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);

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
        clientContext.Gl.BindVertexArray(BillboardVao);

        shader.SetVector2("uTileOffset", new Vector2(tileSize.X * atlasPosition.X, tileSize.Y * atlasPosition.Y));
        shader.SetVector2("uTileSize", tileSize);
        shader.SetMatrix4("uModel", translation);
        clientContext.Gl.DrawElements(PrimitiveType.Triangles, (uint)BillboardIndices.Length, DrawElementsType.UnsignedInt, null);
    }

    public static void CleanUp(ClientContext clientContext)
    {
        clientContext.Gl.DeleteVertexArray(BillboardVao);
        clientContext.Gl.DeleteBuffer(BillboardVbo);
        clientContext.Gl.DeleteBuffer(BillboardEbo);
    }
}