using Kestrel.Framework.Graphics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

public class ClientState
{
    public Kestrel.Framework.Graphics.Window Window;
    public Camera Camera;

    public ClientState(GL gl, IWindow silkWindow)
    {
        Window = new(gl, 800, 600);
        silkWindow.FramebufferResize += @Window.OnResize;

        Camera = new();
    }
}