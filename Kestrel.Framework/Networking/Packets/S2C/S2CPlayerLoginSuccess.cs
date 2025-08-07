using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Server.Player;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets.S2C;

public class S2CPlayerLoginSuccess : IS2CPacket
{
    public ushort PacketId => 2;
    public Vector3 Position;
    public int PlayerCount;
    public List<ClientPlayer> Players;

    public void Deserialize(NetDataReader reader)
    {
        Position.X = reader.GetFloat();
        Position.Y = reader.GetFloat();
        Position.Z = reader.GetFloat();
        PlayerCount = reader.GetInt();
        Players = [];
        for (int i = 0; i < PlayerCount; i++)
        {
            string playerName = reader.GetString(64);
            vec3 location = new()
            {
                x = reader.GetFloat(),
                y = reader.GetFloat(),
                z = reader.GetFloat()
            };

            Players.Add(new ClientPlayer
            {
                Name = playerName,
                Location = location
            });
        }
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Position.X);
        writer.Put(Position.Y);
        writer.Put(Position.Z);
        writer.Put(PlayerCount);
        foreach (var player in Players)
        {
            writer.Put(player.Name, 64);
            writer.Put(player.Location.x);
            writer.Put(player.Location.y);
            writer.Put(player.Location.z);
        }
    }

    public void Handle(ClientState context, NetPeer server)
    {
        context.Player.Location = new vec3(Position.X, Position.X, Position.Y);
        foreach (var player in Players)
        {
            context.Players.Add(player.Name, new ClientPlayer
            {
                Name = player.Name,
                Location = player.Location
            });
        }
    }
}