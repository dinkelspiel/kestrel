using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Client.Graphics;
using Kestrel.Framework.Client.Graphics.Buffers;
using Kestrel.Framework.Server.Player;
using Kestrel.Framework.World;
using LiteNetLib;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

public class ClientState
{
    public Kestrel.Framework.Client.Graphics.Window Window;
    public Camera Camera;
    public ClientPlayer Player;
    public Dictionary<String, ClientPlayer> Players = [];
    public NetPeer NetServer;
    public World World;
    public Dictionary<ChunkPos, ChunkMesh> ChunkMeshes = [];

    public ClientState(GL gl, IWindow silkWindow)
    {
        Window = new(gl, 800, 600);
        silkWindow.FramebufferResize += @Window.OnResize;

        Camera = new(this);
    }
}