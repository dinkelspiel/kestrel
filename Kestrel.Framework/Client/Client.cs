using System.Drawing;
using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Client.Graphics;
using Kestrel.Framework.Client.Graphics.Buffers;
using Kestrel.Framework.Client.Graphics.Shaders;
using Kestrel.Framework.Networking;
using Kestrel.Framework.Networking.Packets;
using Kestrel.Framework.Networking.Packets.C2S;
using Kestrel.Framework.Utils;
using LiteNetLib;
using LiteNetLib.Utils;
using Silk.NET.Core.Contexts;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Kestrel.Framework.Platform;

public class Client
{
    private IWindow _window;
    private IInputContext _input;
    public ClientState clientState;
    private GL _gl;
    private ShaderProgram _shaderProgram;
    private QuadMesh quad;
    NetManager networkClient;
    private DateTime lastRequestedChunks = DateTime.Now;

    public void Run()
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

        if (Environment.GetCommandLineArgs().Length != 2)
        {
            Console.WriteLine("No name provided");
            return;
        }

        clientState = new(_gl, _window)
        {
            Player = new()
            {
                Name = Environment.GetCommandLineArgs()[1],
                Location = new(0, 0, 0),
                LastFrameChunkPos = new(0, 0, 0)
            },
            World = new()
        };

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
        // _gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);
        clientState.Window.GL.Enable(GLEnum.CullFace);
        clientState.Window.GL.CullFace(GLEnum.Back);

        // Tell GL which triangle winding is considered "front"
        clientState.Window.GL.FrontFace(GLEnum.Ccw);

        // Networking

        PacketRegistry.RegisterPackets();

        EventBasedNetListener listener = new();
        networkClient = new(listener);
        networkClient.Start();
        networkClient.Connect("localhost" /* host IP or name */, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
        listener.NetworkReceiveEvent += (server, dataReader, deliveryMethod, channel) =>
        {
            IS2CPacket packet = PacketManager.DeserializeS2CPacket(dataReader);
            PacketManager.HandleS2CPacket(packet, clientState, server);

            dataReader.Recycle();
        };

        listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine("Connected to server!");
            clientState.NetServer = peer;

            C2SPlayerLoginRequest loginRequest = new(clientState.Player.Name);

            clientState.NetServer.Send(PacketManager.SerializeC2SPacket(loginRequest), DeliveryMethod.ReliableOrdered);
        };
    }

    private void OnUpdate(double deltaTime)
    {
        var _keyboard = _input.Keyboards[0];
        float cameraSpeed = 150.0f * (float)deltaTime;

        bool playerMoved = false;
        if (_keyboard.IsKeyPressed(Key.W))
        {
            playerMoved = true;
            clientState.Player.Location += cameraSpeed * clientState.Camera.front.ToVector3();
        }
        if (_keyboard.IsKeyPressed(Key.S))
        {
            playerMoved = true;
            clientState.Player.Location -= cameraSpeed * clientState.Camera.front.ToVector3();
        }
        if (_keyboard.IsKeyPressed(Key.A))
        {
            playerMoved = true;

            clientState.Player.Location -= glm.Normalized(glm.Cross(clientState.Camera.front, clientState.Camera.up)).ToVector3() * cameraSpeed;
        }
        if (_keyboard.IsKeyPressed(Key.D))
        {
            clientState.Player.Location += glm.Normalized(glm.Cross(clientState.Camera.front, clientState.Camera.up)).ToVector3() * cameraSpeed;
            playerMoved = true;
        }

        clientState.World.WorldToChunk((int)clientState.Player.Location.X, (int)clientState.Player.Location.Y, (int)clientState.Player.Location.Z, out var chunkPos, out _);
        bool hasMovedBetweenChunks = !clientState.Player.LastFrameChunkPos.Equals(chunkPos);

        if (playerMoved && clientState.NetServer != null && hasMovedBetweenChunks)
        {
            clientState.Player.LastFrameChunkPos = chunkPos;

            foreach (var (x, y, z) in LocationUtil.CoordsNearestFirst(clientState.RenderDistance, chunkPos.X, chunkPos.Y, chunkPos.Z))
            {
                clientState.RequestChunk(new(x, y, z));
            }

            clientState.NetServer.Send(PacketManager.SerializeC2SPacket(new C2SPlayerMove(clientState.Player.Location)), DeliveryMethod.ReliableUnordered);
        }


        TimeSpan elapsed = DateTime.Now - lastRequestedChunks;
        if (elapsed.TotalMilliseconds > 500)
        {
            lastRequestedChunks = DateTime.Now;
            clientState.RequestChunksFromQueue();
        }

        networkClient.PollEvents();
    }

    private unsafe void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        quad.Draw();
    }

    private void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
        {
            networkClient.Stop();
            _window.Close();
        }

        if (key == Key.F11)
            _window.WindowState = WindowState.Maximized;
    }
}