using System.Drawing;
using Kestrel.Client;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

var app = new KestrelApp();
app.Run();

class KestrelApp
{
    IWindow _window = null!;
    ImGuiController _imGui = null!;
    readonly ClientContext _clientContext = new();

    public void Run()
    {
        var opts = WindowOptions.Default with
        {
            Size = new Vector2D<int>(1280, 720),
            Title = "Kestrel",
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3))
        };

        _window = Window.Create(opts);
        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Update += OnUpdate;
        _window.FramebufferResize += OnFramebufferResize;
        _window.Closing += OnClose;
        _window.Run();
    }

    void OnLoad()
    {
        var gl = _window.CreateOpenGL();
        var input = _window.CreateInput();

        _clientContext.Gl = gl;
        _clientContext.Window = _window;
        _clientContext.Keyboard = input.Keyboards[0];
        _clientContext.Mouse = input.Mice[0];
        _clientContext.Input = new Kestrel.Client.Input.Input(_clientContext.Keyboard);
        _imGui = new ImGuiController(gl, _window, input);

        _clientContext.Keyboard.KeyDown += (kb, key, _) =>
        {
            if (key == Key.Escape) _window.Close();
            if (key == Key.F11)
                _window.WindowState = _window.WindowState == WindowState.Fullscreen
                    ? WindowState.Normal
                    : WindowState.Fullscreen;
        };

        gl.Enable(EnableCap.DepthTest);
        gl.Enable(EnableCap.CullFace);
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        gl.ClearColor(Color.FromArgb(150, 199, 196));

        _clientContext.sceneManager.activeScene.Load();
    }

    void OnUpdate(double dt)
    {
        _imGui.Update((float)dt);
        _clientContext.sceneManager.activeScene.Update(dt);
        _clientContext.Input.NewFrame();
    }

    void OnRender(double dt)
    {
        _clientContext.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _clientContext.sceneManager.activeScene.Render(dt);
        _imGui.Render();
    }

    void OnFramebufferResize(Vector2D<int> size)
    {
        _clientContext.Gl.Viewport(size);
    }

    void OnClose()
    {
        _clientContext.sceneManager.activeScene.Unload();
        _imGui.Dispose();
    }
}
