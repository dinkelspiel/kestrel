using System.Drawing;
using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Graphics;
using Kestrel.Framework.Graphics.Buffers;
using Kestrel.Framework.Graphics.Shaders;
using Silk.NET.Core.Contexts;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Kestrel.Framework.Platform;

public class GameHost
{
    private IWindow _window;
    private IInputContext _input;
    public ClientState clientState;
    private GL _gl;
    private ShaderProgram _shaderProgram;
    private QuadMesh quad;

    public void Run(ClientBase client)
    {
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "My first Silk.NET application!",
        };

        _window = Silk.NET.Windowing.Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
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

        clientState = new(_gl, _window);

        for (int i = 0; i < _input.Keyboards.Count; i++)
        {
            _input.Mice[i].Cursor.CursorMode = CursorMode.Raw;
            _input.Mice[i].MouseMove += clientState.Camera.OnMouseMove;
        }

        VertexShader vs = new(_gl, "./shaders/simple.vs");
        FragmentShader fs = new(_gl, "./shaders/simple.fs");

        _shaderProgram = new(_gl, vs, fs);

        quad = new QuadMesh(clientState, _shaderProgram);

        _gl.Enable(EnableCap.Blend);
        _gl.Enable(EnableCap.DepthTest);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private void OnUpdate(double deltaTime)
    {
        var _keyboard = _input.Keyboards[0];
        const float cameraSpeed = 0.05f;

        if (_keyboard.IsKeyPressed(Key.W))
            clientState.Camera.position += cameraSpeed * clientState.Camera.front;
        if (_keyboard.IsKeyPressed(Key.S))
            clientState.Camera.position -= cameraSpeed * clientState.Camera.front;
        if (_keyboard.IsKeyPressed(Key.A))
            clientState.Camera.position -= glm.Normalized(glm.Cross(clientState.Camera.front, clientState.Camera.up)) * cameraSpeed;
        if (_keyboard.IsKeyPressed(Key.D))
            clientState.Camera.position += glm.Normalized(glm.Cross(clientState.Camera.front, clientState.Camera.up)) * cameraSpeed;

    }

    private unsafe void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        quad.Draw();
    }

    private void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
            _window.Close();

        if (key == Key.F11)
            _window.WindowState = WindowState.Maximized;
    }
}