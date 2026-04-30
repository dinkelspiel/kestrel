using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Kestrel.Client.ECS;
using Kestrel.Client.Renderer;
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
    public Input.Input Input = null!;
    public Camera camera = null!;
    public Entity? Player = null;

    public World World { get; private set; } = World.Create();

    public ClientContext()
    {
        sceneManager = new(this);
    }

    public bool TryGetPlayer(out Entity player)
    {
        if (Player != null)
        {
            player = (Entity)Player;
            return true;
        }
        player = default;
        return false;
    }
}
