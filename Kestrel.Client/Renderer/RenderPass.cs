using System.Numerics;
using Kestrel.Client.ECS;
using Silk.NET.OpenGL;
using Arch.Core.Extensions;

namespace Kestrel.Client.Renderer;

public class RenderPass(ClientContext clientContext)
{
    public uint Vao, Vbo, Ebo;
    public uint BillboardVao, BillboardVbo, BillboardEbo;
    public Shader Shader = null!;
    public Texture Atlas = null!;

    public Vector2 TileSize;

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

    static readonly float[] BillboardVertices =
    [
        -0.5f, -0.5f,  0f,  0f, 1f,
         0.5f, -0.5f,  0f,  1f, 1f,
         0.5f,  0.5f,  0f,  1f, 0f,
        -0.5f,  0.5f,  0f,  0f, 0f,
    ];

    static readonly uint[] BillboardIndices =
    [
         2,  1,  0,   0,  3,  2,
    ];

    public unsafe void Setup()
    {
        // Cube
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

        // Billboard
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

        clientContext.Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
        clientContext.Gl.EnableVertexAttribArray(0);
        clientContext.Gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
        clientContext.Gl.EnableVertexAttribArray(1);


        Atlas = new Texture(clientContext.Gl, Path.Combine(AppContext.BaseDirectory, "Assets", "atlas.png"));
        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders");
        Shader = Shader.FromFiles(clientContext.Gl, Path.Combine(shadersDir, "default.vert"), Path.Combine(shadersDir, "default.frag"));
    }

    public void Begin()
    {
        Shader.Use();
        Shader.SetInt("uTexture", 0);
        TileSize = new Vector2(16f / Atlas.Width, 16f / Atlas.Height);
        Atlas.Bind();
    }

    public unsafe void DrawCube(Matrix4x4 translation, (int X, int Y) atlasPosition)
    {
        var size = clientContext.Window.Size;
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 180f * 65f,
            (float)size.X / size.Y,
            0.1f, 100f);

        Matrix4x4 view = clientContext.camera.GetViewMatrix();

        Shader.SetMatrix4("uView", view);
        Shader.SetMatrix4("uProjection", projection);
        clientContext.Gl.BindVertexArray(Vao);

        Shader.SetVector2("uTileOffset", new Vector2(TileSize.X * atlasPosition.X, TileSize.Y * atlasPosition.Y));
        Shader.SetVector2("uTileSize", TileSize);
        Shader.SetMatrix4("uModel", translation);
        clientContext.Gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
    }

    public unsafe void DrawBillboard(Matrix4x4 translation, (int X, int Y) atlasPosition)
    {
        var size = clientContext.Window.Size;
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 180f * 65f,
            (float)size.X / size.Y,
            0.1f, 100f);

        Matrix4x4 view = clientContext.camera.GetViewMatrix();

        Shader.SetMatrix4("uView", view);
        Shader.SetMatrix4("uProjection", projection);
        clientContext.Gl.BindVertexArray(BillboardVao);

        Shader.SetVector2("uTileOffset", new Vector2(TileSize.X * atlasPosition.X, TileSize.Y * atlasPosition.Y));
        Shader.SetVector2("uTileSize", TileSize);
        Shader.SetMatrix4("uModel", translation * clientContext.camera.GetViewRotation());
        clientContext.Gl.DrawElements(PrimitiveType.Triangles, (uint)BillboardIndices.Length, DrawElementsType.UnsignedInt, null);
    }

    public void End()
    {

    }

    public void CleanUp()
    {
        clientContext.Gl.DeleteVertexArray(Vao);
        clientContext.Gl.DeleteBuffer(Vbo);
        clientContext.Gl.DeleteBuffer(Ebo);
        Atlas.Dispose();
        Shader.Dispose();
    }
}