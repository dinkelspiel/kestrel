using System.Drawing;
using System.Numerics;
using Arch.Core.Extensions;
using GlmSharp;
using Kestrel.Framework.Client.Graphics;
using Kestrel.Framework.Client.Graphics.Buffers;
using Kestrel.Framework.Client.Graphics.Shaders;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.Networking;
using Kestrel.Framework.Networking.Packets;
using Kestrel.Framework.Networking.Packets.C2S;
using Kestrel.Framework.Networking.Packets.S2C;
using Kestrel.Framework.Utils;
using LiteNetLib;
using LiteNetLib.Utils;
using Silk.NET.Core.Contexts;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;

namespace Kestrel.Framework.Platform;

public class Client
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private IWindow _window;
    private IInputContext _input;
    public ClientState clientState;
    private GL _gl;
    private ShaderProgram _shaderProgram;
    private QuadMesh quad;
    NetManager networkClient;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
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
        _gl.ClearColor(Color.FromArgb(1, 121, 184));
        _gl.Viewport(0, 0, (uint)_window.Size.X, (uint)_window.Size.Y);

        if (Environment.GetCommandLineArgs().Length != 2)
        {
            Console.WriteLine("No name provided");
            return;
        }

        clientState = new(_gl, _window)
        {
            PlayerName = Environment.GetCommandLineArgs()[1],
            World = new(),
            Entities = ArchWorld.Create()
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

        ComponentRegistry.RegisterComponents();

        EventBasedNetListener listener = new();
        networkClient = new(listener);
        networkClient.Start();
        networkClient.Connect("localhost" /* host IP or name */, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
        listener.NetworkReceiveEvent += (server, dataReader, deliveryMethod, channel) =>
        {
            var packetId = (Packet)dataReader.GetByte();
            Console.WriteLine("Recieved network packet: {0}", packetId.ToString());
            switch (packetId)
            {
                case Packet.S2CPlayerLoginSuccess:
                    {
                        var packet = new S2CPlayerLoginSuccess();
                        packet.Deserialize(dataReader);

                        Console.WriteLine("Found {0} Entities", packet.EntityCount);

                        bool foundPlayer = false;
                        foreach (var entity in packet.Entities)
                        {
                            Console.WriteLine("Parsing Entity");
                            ArchEntity archEntity = clientState.Entities.Create(new ServerId(entity.Key));

                            // entity.Key is the server ID so we add it to the dictionary
                            clientState.ServerIdToEntity.TryAdd(entity.Key, archEntity);

                            foreach (var component in entity.Value)
                            {
                                Console.WriteLine("Component Type: {0} {1}", component.PacketId, component.GetType());
                                if (component is Player player && player.Name == clientState.PlayerName)
                                {
                                    clientState.Player = archEntity;
                                    foundPlayer = true;
                                }
                                switch (component)
                                {
                                    case Player p: clientState.Entities.Add(archEntity, p); break;
                                    case Location l: clientState.Entities.Add(archEntity, l); break;
                                    case Nametag n: clientState.Entities.Add(archEntity, n); break;
                                    case Velocity v: clientState.Entities.Add(archEntity, v); break;
                                    default:
                                        Console.WriteLine($"Unknown component {component.GetType().Name}");
                                        break;
                                }
                            }
                        }

                        if (!foundPlayer)
                        {
                            Console.WriteLine("No player was sent from the server, exiting.");
                            Environment.Exit(0);
                            return;
                        }

                        Console.WriteLine("Player has {0} components.", clientState.Entities.GetAllComponents(clientState.Player).Length);

                        clientState.status = ClientStatus.Connected;

                        Location position = clientState.Player.Get<Location>();

                        clientState.World.WorldToChunk((int)position.X, (int)position.Y, (int)position.Z, out var chunkPos, out _);
                        position.LastFrameChunkPos = chunkPos;

                        foreach (var (x, y, z) in LocationUtil.CoordsNearestFirst(clientState.RenderDistance, chunkPos.X, chunkPos.Y, chunkPos.Z))
                        {
                            clientState.RequestChunk(new(x, y, z));
                        }
                    }
                    break;
                case Packet.S2CBroadcastEntitySpawn:
                    {
                        var packet = new S2CBroadcastEntitySpawn();
                        packet.Deserialize(dataReader);

                        var playerServerId = clientState.Player.Get<ServerId>().Id;
                        if (packet.ServerId == playerServerId)
                        {
                            return;
                        }

                        if (!clientState.ServerIdToEntity.ContainsKey(packet.ServerId))
                        {
                            ArchEntity archEntity = clientState.Entities.Create(new ServerId(packet.ServerId));

                            // entity.Key is the server ID so we add it to the dictionary
                            clientState.ServerIdToEntity.TryAdd(packet.ServerId, archEntity);

                            foreach (var component in packet.Components)
                            {
                                // Should be unreachable but you never know
                                if (component is Player player && player.Name == clientState.PlayerName)
                                {
                                    clientState.Player = archEntity;
                                }
                                clientState.Entities.Add(archEntity, component);
                            }
                        }
                        else
                        {
                            // If the entity already exists we just ignore it for now we might want to change this later
                            // to check for new components etc etc
                        }
                    }
                    break;
                case Packet.S2CBroadcastEntityMove:
                    {
                        var packet = new S2CBroadcastEntityMove();
                        packet.Deserialize(dataReader);

                        // Don't update the position of the player, might want to change this later but yeah
                        var playerServerId = clientState.Player.Get<ServerId>().Id;
                        if (packet.ServerId == playerServerId)
                        {
                            return;
                        }

                        ArchEntity entity = clientState.ServerIdToEntity[packet.ServerId];
                        entity.Get<Location>().Postion = new Vector3(packet.Position.X, packet.Position.Y, packet.Position.Z);
                    }
                    break;
                case Packet.S2CChunkResponse:
                    {
                        var packet = new S2CChunkResponse();
                        packet.Deserialize(dataReader);

                        foreach (var packetChunk in packet.Chunks)
                        {
                            var chunk = new World.Chunk(clientState.World, packetChunk.ChunkX, packetChunk.ChunkY, packetChunk.ChunkZ) { Blocks = packetChunk.Blocks, IsEmpty = packetChunk.IsEmpty };
                            clientState.World.SetChunk(packetChunk.ChunkX, packetChunk.ChunkY, packetChunk.ChunkZ, chunk);

                            var key = new Vector3I(packetChunk.ChunkX, packetChunk.ChunkY, packetChunk.ChunkZ);
                            clientState.ChunkMeshes.Remove(key, out var _);

                            var mesh = new ChunkMesh(clientState, chunk);
                            clientState.ChunkMeshManager.QueueGeneration(mesh);
                            clientState.ChunkMeshes.TryAdd(key, mesh);

                            if (clientState.ChunkMeshes.TryGetValue(new Vector3I(packetChunk.ChunkX, packetChunk.ChunkY + 1, packetChunk.ChunkZ), out var topMesh)) clientState.ChunkMeshManager.QueueGeneration(topMesh);
                            if (clientState.ChunkMeshes.TryGetValue(new Vector3I(packetChunk.ChunkX, packetChunk.ChunkY - 1, packetChunk.ChunkZ), out var bottomMesh)) clientState.ChunkMeshManager.QueueGeneration(bottomMesh);
                            if (clientState.ChunkMeshes.TryGetValue(new Vector3I(packetChunk.ChunkX, packetChunk.ChunkY, packetChunk.ChunkZ + 1), out var northMesh)) clientState.ChunkMeshManager.QueueGeneration(northMesh);
                            if (clientState.ChunkMeshes.TryGetValue(new Vector3I(packetChunk.ChunkX, packetChunk.ChunkY, packetChunk.ChunkZ - 1), out var southMesh)) clientState.ChunkMeshManager.QueueGeneration(southMesh);
                            if (clientState.ChunkMeshes.TryGetValue(new Vector3I(packetChunk.ChunkX - 1, packetChunk.ChunkY, packetChunk.ChunkZ), out var westMesh)) clientState.ChunkMeshManager.QueueGeneration(westMesh);
                            if (clientState.ChunkMeshes.TryGetValue(new Vector3I(packetChunk.ChunkX + 1, packetChunk.ChunkY, packetChunk.ChunkZ), out var eastMesh)) clientState.ChunkMeshManager.QueueGeneration(eastMesh);
                        }
                    }
                    break;
            }

            dataReader.Recycle();
        };

        listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine("Connected to server!");
            clientState.NetServer = peer;

            C2SPlayerLoginRequest loginRequest = new(clientState.PlayerName);

            clientState.NetServer.Send(IPacket.Serialize(loginRequest), DeliveryMethod.ReliableOrdered);
        };
    }

    private void OnUpdate(double deltaTime)
    {
        clientState.Profiler.Tick += 1;

        networkClient.PollEvents();

        if (clientState.status != ClientStatus.Connected)
            return;

        var _keyboard = _input.Keyboards[0];
        float cameraSpeed = 150.0f * (float)deltaTime;

        clientState.Entities.Query(new Arch.Core.QueryDescription().WithAll<Location>(), (ArchEntity entity, ref Location location) =>
        {
            bool playerMoved = false;
            if (_keyboard.IsKeyPressed(Key.W))
            {
                playerMoved = true;
                location.Postion += cameraSpeed * clientState.Camera.front.ToVector3();
            }
            if (_keyboard.IsKeyPressed(Key.S))
            {
                playerMoved = true;
                location.Postion -= cameraSpeed * clientState.Camera.front.ToVector3();
            }
            if (_keyboard.IsKeyPressed(Key.A))
            {
                playerMoved = true;

                location.Postion -= glm.Normalized(glm.Cross(clientState.Camera.front, clientState.Camera.up)).ToVector3() * cameraSpeed;
            }
            if (_keyboard.IsKeyPressed(Key.D))
            {
                location.Postion += glm.Normalized(glm.Cross(clientState.Camera.front, clientState.Camera.up)).ToVector3() * cameraSpeed;
                playerMoved = true;
            }

            clientState.World.WorldToChunk((int)location.X, (int)location.Y, (int)location.Z, out var chunkPos, out _);
            bool hasMovedBetweenChunks = !location.LastFrameChunkPos.Equals(chunkPos);

            if (playerMoved && clientState.NetServer != null && hasMovedBetweenChunks)
            {
                clientState.Profiler.Start("Requested chunks distance culling", () =>
                {
                    List<Vector3I> _requestedChunksCache = [.. clientState.RequestedChunksQueue];
                    foreach (var targetChunk in _requestedChunksCache)
                    {
                        bool condition = LocationUtil.Distance(chunkPos.ToVector3(), targetChunk.ToVector3()) > clientState.RenderDistance * 1.5;
                        if (condition)
                        {
                            clientState.RequestedChunks.Remove(targetChunk);
                            clientState.RequestedChunksQueue.Remove(targetChunk);
                        }
                    }
                });


                // List<KeyValuePair<Vector3I, ChunkMesh>> _chunkMeshes = clientState.ChunkMeshes.ToList();
                // foreach (var targetChunk in _chunkMeshes)
                // {
                //     bool condition = LocationUtil.Distance(chunkPos.ToVector3(), targetChunk.Key.ToVector3()) > clientState.RenderDistance * 1.5;
                //     if (condition)
                //     {
                //         clientState.ChunkMeshes.Remove(targetChunk.Key);
                //     }
                // }

                location.LastFrameChunkPos = chunkPos;

                foreach (var (x, y, z) in LocationUtil.CoordsNearestFirst(clientState.RenderDistance, chunkPos.X, chunkPos.Y, chunkPos.Z))
                {
                    clientState.RequestChunk(new(x, y, z));
                }

                clientState.NetServer.Send(IPacket.Serialize(new C2SPlayerMove(location.Postion)), DeliveryMethod.ReliableUnordered);
            }
        });

        clientState.Profiler.Start("Request chunks from queue", () =>
        {
            TimeSpan elapsed = DateTime.Now - lastRequestedChunks;
            if (elapsed.TotalMilliseconds > 500)
            {
                lastRequestedChunks = DateTime.Now;
                clientState.RequestChunksFromQueue();
            }
        });

        clientState.Profiler.Start("Generate chunks meshes under limit", () =>
        {
            clientState.ChunkMeshManager.GenerateFromQueueUnderTimeLimit(8);
        });
    }

    private unsafe void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (clientState.status != ClientStatus.Connected)
            return;

        clientState.Profiler.Start("Rendering", () =>
        {
            quad.Draw();
        });
    }

    private void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
        {
            networkClient.Stop();
            _window.Close();
            clientState.Profiler.Build();
        }

        if (key == Key.F11)
            _window.WindowState = WindowState.Maximized;
    }
}