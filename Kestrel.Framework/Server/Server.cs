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
                            player = ServerState.Entities.Create(new ServerId(guid), new Entity.Components.Player(packet.PlayerName!), new Location(ServerState.World, -416, 80, 383), new Nametag(packet.PlayerName!), new Velocity(0, 0, 0));

                            ServerState.PlayersByName.TryAdd(packet.PlayerName!, player);
                            ServerState.PlayersByConnection.TryAdd(client, player);
                            ServerState.NetworkableEntities.TryAdd(guid, player);
                        }

                        var serializedEntities = new Dictionary<Guid, INetworkableComponent[]>();
                        Console.WriteLine("Found {0} eligible entities to send.", ServerState.NetworkableEntities.Count);
                        foreach (var entity in ServerState.NetworkableEntities)
                        {
                            List<INetworkableComponent> serializedComponents = [];
                            var entityComponents = ServerState.Entities.GetAllComponents(entity.Value);

                            foreach (var _component in entityComponents)
                            {
                                if (_component is INetworkableComponent component)
                                {
                                    serializedComponents.Add(component);
                                }
                            }
                            serializedEntities.Add(entity.Key, [.. serializedComponents]);
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
                            generatedChunks[i] = ServerState.World.GetChunkOrGenerate(chunkPos.X, chunkPos.Y, chunkPos.Z);
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
                        player.Get<Location>().Postion = packet.Location;
                        var playerServerId = player.Get<ServerId>().Id;

                        ServerState.NetServer.SendToAll(IPacket.Serialize(new S2CBroadcastEntityMove()
                        {
                            ServerId = playerServerId,
                            Position = new Vector3(packet.Location.X, packet.Location.Y, packet.Location.Z)
                        }), DeliveryMethod.ReliableOrdered);
                    }
                    break;
            }

            dataReader.Recycle();
        }
                ;


        while (!Console.KeyAvailable)
        {
            ServerState.NetServer.PollEvents();

            ServerState.Entities.Query(new QueryDescription().WithAll<Location>(), (ArchEntity entity, ref Location location) =>
            {
                if (LocationUtil.Distance(new(location.X, location.Y, location.Z), new(location.LastUpdatedX, location.LastUpdatedY, location.LastUpdatedZ)) > 5)
                {

                }
            });
        }
        ServerState.NetServer.Stop();
    }
}
