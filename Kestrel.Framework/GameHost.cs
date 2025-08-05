using System.Drawing;
using Kestrel.Framework.Buffers;
using Kestrel.Framework.Shaders;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Kestrel.Framework.Platform;

public class GameHost
{
    private IWindow _window;
    private IInputContext _input;
    private GL _gl;
    private ShaderProgram _shaderProgram;
    private QuadMesh quad;

    public void Run(ClientBase client)
    {
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "My first Silk.NET application!"
        };

        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += size => _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        _window.Run();
    }

    private unsafe void OnLoad()
    {
        _input = _window.CreateInput();
        for (int i = 0; i < _input.Keyboards.Count; i++)
            _input.Keyboards[i].KeyDown += KeyDown;

        _gl = _window.CreateOpenGL();
        _gl.ClearColor(Color.CornflowerBlue);
        _gl.Viewport(0, 0, (uint)_window.Size.X, (uint)_window.Size.Y);

        VertexShader vs = new(_gl, "./shaders/simple.vs");
        FragmentShader fs = new(_gl, "./shaders/simple.fs");

        _shaderProgram = new(_gl, vs, fs);

        quad = new QuadMesh(_gl, _shaderProgram);

        new ElementBuffer(_gl, [
            0u, 1u, 3u,
            1u, 2u, 3u
        ]).Bind();

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    private void OnUpdate(double deltaTime) { }

    private unsafe void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        quad.Draw();
    }

    private void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
            _window.Close();
    }
}