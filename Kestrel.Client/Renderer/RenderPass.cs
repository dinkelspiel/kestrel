using System.Numerics;
using Kestrel.Client.ECS;
using Silk.NET.OpenGL;
using Arch.Core.Extensions;

namespace Kestrel.Client.Renderer;

public class RenderPass(ClientContext clientContext)
{
    public uint Vao, Vbo, Ebo;
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


    public unsafe void Setup()
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

        Atlas = new Texture(clientContext.Gl, Path.Combine(AppContext.BaseDirectory, "Assets", "atlas.png"));
        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders");
        Shader = Shader.FromFiles(clientContext.Gl, Path.Combine(shadersDir, "default.vert"), Path.Combine(shadersDir, "default.frag"));
        Shader.Use();
        Shader.SetInt("uTexture", 0);
        TileSize = new Vector2(16f / Atlas.Width, 16f / Atlas.Height);
    }

    public void Begin()
    {
        Shader.Use();
    }

    public unsafe void DrawCube(Matrix4x4 translation, (int X, int Y) atlasPosition)
    {
        var cameraTarget = Vector3.Zero;
        if (clientContext.TryGetPlayer(out var player))
            cameraTarget = player.Get<TransformComponent>().Postition;

        float yawRad = MathF.PI / 180f * clientContext.camera.Yaw;
        float pitchRad = MathF.PI / 180f * clientContext.camera.Pitch;
        var cameraOffset = new Vector3(
            clientContext.camera.CameraDistance * MathF.Cos(pitchRad) * MathF.Cos(yawRad),
            clientContext.camera.CameraDistance * MathF.Sin(pitchRad),
            clientContext.camera.CameraDistance * MathF.Cos(pitchRad) * MathF.Sin(yawRad)
        );
        var cameraRight = Vector3.Normalize(Vector3.Cross(-cameraOffset, Vector3.UnitY));
        var shoulder = cameraRight * clientContext.camera.CameraShoulderOffset;
        var head = Vector3.UnitY * clientContext.camera.CameraHeightOffset;
        var cameraPos = cameraTarget + cameraOffset + shoulder + head;
        var view = Matrix4x4.CreateLookAt(cameraPos, cameraTarget + shoulder + head, Vector3.UnitY);

        var size = clientContext.Window.Size;
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 180f * 65f,
            (float)size.X / size.Y,
            0.1f, 100f);

        Shader.SetMatrix4("uView", view);
        Shader.SetMatrix4("uProjection", projection);
        Atlas.Bind();
        clientContext.Gl.BindVertexArray(Vao);

        Shader.SetVector2("uTileOffset", new Vector2(TileSize.X * atlasPosition.X, TileSize.Y * atlasPosition.Y));
        Shader.SetVector2("uTileSize", TileSize);
        Shader.SetMatrix4("uModel", translation);
        clientContext.Gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
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