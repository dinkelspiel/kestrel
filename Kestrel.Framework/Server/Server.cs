using System.Collections.Concurrent;
using Arch.Core;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.Networking;
using Kestrel.Framework.Networking.Packets;
using Kestrel.Framework.Networking.Packets.C2S;
using Kestrel.Framework.Utils;
using LiteNetLib;
using LiteNetLib.Utils;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;
using Silk.NET.Core;
using System.Numerics;
using Kestrel.Framework.Networking.Packets.S2C;
using Arch.Core.Extensions;
using Kestrel.Framework.Entity;
using System.Reflection.Metadata.Ecma335;
using System.Net;
using Kestrel.Framework.World;

namespace Kestrel.Framework.Server;

public class Server
{
    public ServerState ServerState { get; private set; } = new();

    public void Run()
    {
        EventBasedNetListener listener = new();
        ServerState.NetServer = new(listener);
        ServerState.NetServer.Start(9050 /* port */);

        ServerState.World = new();
        ServerState.Entities = ArchWorld.Create();

        ServerState.Entities.Create(new Location(ServerState.World, -416, 80, 383), new ModelRenderer(""));

        listener.ConnectionRequestEvent += request =>
        {
            // if (ServerState.NetServer.ConnectedPeersCount < 10 /* max connections */)
            request.AcceptIfKey("SomeConnectionKey");
            // else
            // request.Reject();
        };

        listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine("We got connection: {0}", peer);  // Show peer ip

            NetDataWriter writer = new();
            peer.Send(writer, DeliveryMethod.ReliableOrdered);  // Send with reliability
        };

        listener.NetworkReceiveEvent += (client, dataReader, deliveryMethod, channel) =>
        {
            var packetId = (Packet)dataReader.GetByte();
            Console.WriteLine("Recieved network packet: {0}", packetId.ToString());
            switch (packetId)
            {
                case Packet.C2SPlayerLoginRequest:
                    {
                        var packet = new C2SPlayerLoginRequest();
                        packet.Deserialize(dataReader);

                        var playerExists = ServerState.PlayersByName.TryGetValue(packet.PlayerName!, out ArchEntity player);
                        if (playerExists)
                        {
                            var existingPlayer = ServerState.PlayersByConnection.Where((kvp) => kvp.Value.Id == player.Id);
                            if (existingPlayer.Any())
                            {
                                existingPlayer.First().Key.Disconnect();
                                ServerState.PlayersByConnection.TryRemove(existingPlayer.First());
                                Console.WriteLine("Disconnected logged in player for {0}", packet.PlayerName);
                            }

                            ServerState.PlayersByConnection.TryAdd(client, player);
                        }
                        else
                        {
                            var guid = Guid.NewGuid();
                            player = ServerState.Entities.Create(new ServerId(guid), new Entity.Components.Player(packet.PlayerName!), new Location(ServerState.World, -416, 100, 383), new Nametag(packet.PlayerName!), new Velocity(0, 0, 0), new Physics());

                            ServerState.PlayersByName.TryAdd(packet.PlayerName!, player);
                            ServerState.PlayersByConnection.TryAdd(client, player);
                            ServerState.NetworkableEntities.TryAdd(guid, player);
                        }

                        var serializedEntities = new Dictionary<Guid, INetworkableComponent[]>();
                        Console.WriteLine("Found {0} eligible entities to send.", ServerState.NetworkableEntities.Count);
                        foreach (var entity in ServerState.NetworkableEntities)
                        {
                            var serializedComponents = ServerState.GetNetworkableComponents(entity.Value);
                            serializedEntities.Add(entity.Key, serializedComponents);
                        }
                        var loginSuccess = new S2CPlayerLoginSuccess
                        {
                            Entities = serializedEntities,
                            EntityCount = serializedEntities.Count
                        };
                        client.Send(IPacket.Serialize(loginSuccess), DeliveryMethod.ReliableOrdered);
                    }
                    break;
                case Packet.C2SChunkRequest:
                    {
                        var packet = new C2SChunkRequest();
                        packet.Deserialize(dataReader);

                        var chunks = packet.Chunks ?? throw new Exception("packet.Chunks was null in chunk request packet");
                        var generatedChunks = new World.Chunk[packet.ChunkCount];

                        Parallel.For(0, packet.ChunkCount, new ParallelOptions
                        {
                            MaxDegreeOfParallelism = Environment.ProcessorCount
                        }, (i) =>
                        {
                            var chunkPos = chunks[i];
                            var chunk = ServerState.World.GetChunkOrGenerate(chunkPos.X, chunkPos.Y, chunkPos.Z, out var generated);
                            generatedChunks[i] = chunk;
                        });

                        client.Send(IPacket.Serialize(new S2CChunkResponse(generatedChunks)), DeliveryMethod.ReliableUnordered);
                    }
                    break;
                case Packet.C2SPlayerMove:
                    {
                        var packet = new C2SPlayerMove();
                        packet.Deserialize(dataReader);

                        if (!ServerState.PlayersByConnection.TryGetValue(client, out ArchEntity player))
                        {
                            Console.WriteLine($"Client not found in context.");
                            return;
                        }
                        player.Get<Location>().Position = packet.Location;
                        var playerServerId = player.Get<ServerId>().Id;

                        ServerState.NetServer.SendToAll(IPacket.Serialize(new S2CBroadcastEntityMove()
                        {
                            ServerId = playerServerId,
                            Position = new Vector3(packet.Location.X, packet.Location.Y, packet.Location.Z)
                        }), DeliveryMethod.Unreliable);
                    }
                    break;
            }

            dataReader.Recycle();
        }
                ;

        const int targetTickRate = 20;          // 20 ticks per second
        const double tickInterval = 1000.0 / targetTickRate; // in ms
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        double accumulated = 0;

        while (!Console.KeyAvailable)
        {
            ServerState.NetServer.PollEvents();

            accumulated += stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();

            while (accumulated >= tickInterval)
            {
                Tick();
                accumulated -= tickInterval;
            }

            // Thread.Sleep(1);
        }
        ServerState.NetServer.Stop();
    }

    public void Tick()
    {
        ServerState.Entities.Query(new QueryDescription().WithAll<Velocity, Physics>(), (ArchEntity entity, ref Velocity velocity, ref Physics physics) =>
        {
            if (entity.Has<Player>())
                return;

            velocity.Y += Physics.GRAVITY;

            // Cap y velocity
            velocity.Y = MathF.Min(velocity.Y, 3);
        });

        ServerState.Entities.Query(new QueryDescription().WithAll<EntityAi, Location>(), (ArchEntity entity, ref EntityAi entityAi, ref Location location) =>
        {
            var secondsSinceLastStateChange = (DateTime.Now - entityAi.LastStateChange).Milliseconds / 1000f;
            if (entityAi.State.StateTime < secondsSinceLastStateChange)
            {
                var random = new Random();
                if (random.NextDouble() < 0.5)
                {
                    entityAi.State = new EntityIdle();
                }
                else
                {
                    List<Vector3> eligbleLocations = [];
                    for (int wx = (int)location.X - 8; wx < (int)location.X + 8; wx++)
                    {
                        for (int wy = (int)location.Y - 8; wy < (int)location.Y + 8; wy++)
                        {
                            for (int wz = (int)location.Z - 8; wz < (int)location.Z + 8; wz++)
                            {

                                BlockType? block = ServerState.World.GetBlock(wx, wy, wz);
                                BlockType? blockAbove = ServerState.World.GetBlock(wx, wy + 1, wz);
                                BlockType? blockAbove2 = ServerState.World.GetBlock(wx, wy + 2, wz);
                                if (!block.HasValue || !blockAbove.HasValue || !blockAbove2.HasValue)
                                    continue;

                                if (block.Value.IsSolid() && blockAbove == BlockType.Air && blockAbove2 == BlockType.Air)
                                    eligbleLocations.Add(new(wx, wy, wz));

                            }
                        }
                    }
                    if (eligbleLocations.Count == 0)
                        entityAi.State = new EntityIdle();
                    else
                        entityAi.State = new EntityWalking(eligbleLocations[random.Next(0, eligbleLocations.Count)]);
                }
            }
        });

        ServerState.Entities.Query(
            new QueryDescription().WithAll<Location, Velocity, Collider>(),
            (ArchEntity entity, ref Location location, ref Velocity velocity, ref Collider collider) =>
        {
            if (entity.Has<Player>())
                return;

            Vector3 startPos = location.Position;

            collider.IsOnGround = false;

            // TODO: Add deltatime
            Vector3 move = velocity.Vel;

            AxisResolver.MoveAxis(ServerState.World, ref location, ref velocity, ref collider, AxisResolver.Axis.Y, move.Y);
            AxisResolver.MoveAxis(ServerState.World, ref location, ref velocity, ref collider, AxisResolver.Axis.X, move.X);
            AxisResolver.MoveAxis(ServerState.World, ref location, ref velocity, ref collider, AxisResolver.Axis.Z, move.Z);

            Vector3 displacement = location.Position - startPos;
            float distanceMoved = displacement.Length();
            velocity.DistanceMoved += distanceMoved;


            if (velocity.DistanceMoved > 1f && entity.Has<ServerId>())
            {
                var serverId = entity.Get<ServerId>();

                var packet = new S2CBroadcastEntityMove
                {
                    ServerId = serverId.Id,
                    Position = location.Position
                };

                velocity.DistanceMoved = 0f;
                ServerState.NetServer.SendToAll(IPacket.Serialize(packet), DeliveryMethod.Unreliable);
            }
        });
    }
}
