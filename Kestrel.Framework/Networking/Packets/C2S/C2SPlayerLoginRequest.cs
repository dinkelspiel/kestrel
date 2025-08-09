using System.Numerics;
using System.Text;
using GlmSharp;
using Kestrel.Framework.Networking.Packets.S2C;
using Kestrel.Framework.Server;
using Kestrel.Framework.Server.Player;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets.C2S;

public struct C2SPlayerLoginRequest(String playerName) : IC2SPacket
{
    public String PlayerName = playerName;

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
            vec3 location = new(0, 120, 0);
            ServerPlayer serverPlayer = new ServerPlayer()
            {
                Name = PlayerName,
                Location = location,
                NetClient = client
            };

            context.PlayersByName.TryAdd(PlayerName, serverPlayer);
            context.PlayersByConnection.TryAdd(client, serverPlayer);

            client.Send(PacketManager.SerializeS2CPacket(new S2CPlayerLoginSuccess()
            {
                Position = new(location.x, location.y, location.z),
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