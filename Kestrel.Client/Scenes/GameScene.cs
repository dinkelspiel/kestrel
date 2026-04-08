using System.Numerics;
using Kestrel.Client.Scene;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Shader = Kestrel.Client.Renderer.Shader;
using Texture = Kestrel.Client.Renderer.Texture;

namespace Kestrel.Client.Scenes;

public class GameScene(ClientContext clientContext) : SceneBase(clientContext)
{
    uint _vao, _vbo, _ebo;
    Shader _shader = null!;
    Texture _atlas = null!;

    Vector3 _playerPos = new(0f, 0.5f, 0f);
    float _yaw = -90f;
    float _pitch = 20f;
    float _lastMouseX, _lastMouseY;
    bool _firstMouse = true;
    float _sensitivity = 0.1f;
    float _speed = 4f;
    float _cameraDistance = 9f;

    Vector2 _tileSize;

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

    public override unsafe void Load()
    {
        var gl = clientContext.Gl;

        clientContext.Mouse.Cursor.CursorMode = CursorMode.Raw;
        clientContext.Mouse.MouseMove += OnMouseMove;

        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* v = Vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(Vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);

        _ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* i = Indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);

        const uint stride = 5 * sizeof(float);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(1);

        _atlas = new Texture(gl, Path.Combine(AppContext.BaseDirectory, "Assets", "atlas.png"));
        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders");
        _shader = Shader.FromFiles(gl, Path.Combine(shadersDir, "default.vert"), Path.Combine(shadersDir, "default.frag"));
        _shader.Use();
        _shader.SetInt("uTexture", 0);
        _tileSize = new Vector2(16f / _atlas.Width, 16f / _atlas.Height);
    }

    public override void Update(double dt)
    {
        var keyboard = clientContext.Keyboard;
        float velocity = _speed * (float)dt;

        float yawRad = MathF.PI / 180f * _yaw;
        var forward = Vector3.Normalize(new Vector3(MathF.Cos(yawRad), 0f, MathF.Sin(yawRad)));
        var right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));

        if (keyboard.IsKeyPressed(Key.W)) _playerPos -= velocity * forward;
        if (keyboard.IsKeyPressed(Key.S)) _playerPos += velocity * forward;
        if (keyboard.IsKeyPressed(Key.A)) _playerPos += right * velocity;
        if (keyboard.IsKeyPressed(Key.D)) _playerPos -= right * velocity;
        if (keyboard.IsKeyPressed(Key.Space)) _playerPos.Y += velocity;
        if (keyboard.IsKeyPressed(Key.ShiftLeft)) _playerPos.Y -= velocity;
    }

    public override unsafe void Render(double dt)
    {
        var gl = clientContext.Gl;
        _shader.Use();

        float yawRad = MathF.PI / 180f * _yaw;
        float pitchRad = MathF.PI / 180f * _pitch;
        var cameraOffset = new Vector3(
            _cameraDistance * MathF.Cos(pitchRad) * MathF.Cos(yawRad),
            _cameraDistance * MathF.Sin(pitchRad),
            _cameraDistance * MathF.Cos(pitchRad) * MathF.Sin(yawRad)
        );
        var cameraPos = _playerPos + cameraOffset;
        var view = Matrix4x4.CreateLookAt(cameraPos, _playerPos, Vector3.UnitY);

        var size = clientContext.Window.Size;
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 180f * 65f,
            (float)size.X / size.Y,
            0.1f, 100f);

        _shader.SetMatrix4("uView", view);
        _shader.SetMatrix4("uProjection", projection);
        _atlas.Bind();
        gl.BindVertexArray(_vao);

        _shader.SetVector2("uTileOffset", new Vector2(0f, 0f));
        _shader.SetVector2("uTileSize", _tileSize);
        _shader.SetMatrix4("uModel", Matrix4x4.Identity);
        gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);

        _shader.SetVector2("uTileOffset", new Vector2(_tileSize.X, 0f));
        var playerModel = Matrix4x4.CreateRotationY(MathF.PI - yawRad) * Matrix4x4.CreateTranslation(_playerPos);
        _shader.SetMatrix4("uModel", playerModel);
        gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
    }

    public override void Unload()
    {
        clientContext.Mouse.MouseMove -= OnMouseMove;
        var gl = clientContext.Gl;
        gl.DeleteVertexArray(_vao);
        gl.DeleteBuffer(_vbo);
        gl.DeleteBuffer(_ebo);
        _atlas.Dispose();
        _shader.Dispose();
    }

    void OnMouseMove(IMouse mouse, Vector2 position)
    {
        if (_firstMouse)
        {
            _lastMouseX = position.X;
            _lastMouseY = position.Y;
            _firstMouse = false;
            return;
        }

        float dx = (position.X - _lastMouseX) * _sensitivity;
        float dy = (_lastMouseY - position.Y) * _sensitivity;
        _lastMouseX = position.X;
        _lastMouseY = position.Y;

        _yaw += dx;
        _pitch = Math.Clamp(_pitch + dy, 5f, 80f);
    }
}
