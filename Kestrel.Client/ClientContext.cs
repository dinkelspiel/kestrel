using Kestrel.Client.Scene;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Kestrel.Client;

public class ClientContext
{
    public SceneManager sceneManager;
    public GL Gl = null!;
    public IWindow Window = null!;
    public IKeyboard Keyboard = null!;
    public IMouse Mouse = null!;

    public ClientContext()
    {
        sceneManager = new(this);
    }
}