using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Server.Player;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets.S2C;

public class S2CBroadcastPlayerMove : IS2CPacket
{
    public ushort PacketId => 4;
    public string PlayerName;
    public Vector3 Position;

    public void Deserialize(NetDataReader reader)
    {
        PlayerName = reader.GetString(64);
        Position = new()
        {
            X = reader.GetFloat(),
            Y = reader.GetFloat(),
            Z = reader.GetFloat()
        };
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PlayerName, 64);
        writer.Put(Position.X);
        writer.Put(Position.Y);
        writer.Put(Position.Z);
    }

    public void Handle(ClientState context, NetPeer server)
    {
        if (PlayerName == context.Player.Name)
        {
            return;
        }

        ClientPlayer player = context.Players[PlayerName];
        player.Location = new Vector3(Position.X, Position.Y, Position.Z);
    }
}