using System.Numerics;
using System.Text;
using GlmSharp;
using Kestrel.Framework.Entity;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.Networking.Packets.S2C;
using Kestrel.Framework.Server;
using Kestrel.Framework.Server.Player;
using LiteNetLib;
using LiteNetLib.Utils;
using ArchEntity = Arch.Core.Entity;

namespace Kestrel.Framework.Networking.Packets.C2S;

public struct C2SPlayerLoginRequest(string playerName) : IC2SPacket
{
    public string PlayerName = playerName;

    public readonly ushort PacketId => 1;

    public void Deserialize(NetDataReader reader)
    {
        // Console.WriteLine($"offset: {reader.UserDataOffset} {reader.AvailableBytes}");
        // byte[] guidBytes = new byte[16];
        // reader.GetBytes(guidBytes, 16);
        // playerGuid = new Guid(guidBytes);
        PlayerName = reader.GetString(64);
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PlayerName, 64);
    }

    public void Handle(ServerState context, NetPeer client)
    {
        if (!context.PlayersByName.ContainsKey(PlayerName))
        {
            vec3 location = new();
            ServerPlayer serverPlayer = new()
            {
                Name = PlayerName,
                Location = location,
                NetClient = client
            };

            ArchEntity player = context.Entities.Create(new Player(PlayerName), new Location(-416, 80, 383), new Nametag(PlayerName), new Velocity(0, 0, 0));

            context.PlayersByName.TryAdd(PlayerName, player);
            context.PlayersByConnection.TryAdd(client, player);

            Dictionary<int, INetworkableComponent[]> packetEntities = [];

            context.Entities.Query(new Arch.Core.QueryDescription().WithAll<INetworkableComponent>(), (ArchEntity entity, ref INetworkableComponent component) =>
            {
                
            });

            client.Send(PacketManager.SerializeS2CPacket(new S2CPlayerLoginSuccess()
            {
                EntityCount = context.Entities.CountEntities(new Arch.Core.QueryDescription().WithAll<INetworkableComponent>()),
                Entities = 
            }), DeliveryMethod.ReliableOrdered);

            context.NetServer.SendToAll(PacketManager.SerializeS2CPacket(new S2CBroadcastPlayerJoin()
            {
                PlayerName = PlayerName,
                Position = new(location.x, location.y, location.z)
            }), DeliveryMethod.ReliableOrdered);

            Console.WriteLine($"Player {PlayerName} logged in successfully.");
        }
        else
        {
            ServerPlayer player = context.PlayersByName[PlayerName];
            player.NetClient = client;
            context.PlayersByConnection.TryAdd(client, player);

            Vector3 location = new(player.Location.x, player.Location.y, player.Location.z);
            client.Send(PacketManager.SerializeS2CPacket(new S2CPlayerLoginSuccess()
            {
                Position = location,
                PlayerCount = context.PlayersByName.Count,
                Players = context.PlayersByName.Values.Select(p => new ClientPlayer
                {
                    Name = p.Name,
                    Location = new Vector3(p.Location.x, p.Location.y, p.Location.z)
                }).ToList()
            }), DeliveryMethod.ReliableOrdered);

            context.NetServer.SendToAll(PacketManager.SerializeS2CPacket(new S2CBroadcastPlayerJoin()
            {
                PlayerName = PlayerName,
                Position = location
            }), DeliveryMethod.ReliableOrdered);

            Console.WriteLine($"Player {PlayerName} already exists. Sending existing player.");
        }
    }
}