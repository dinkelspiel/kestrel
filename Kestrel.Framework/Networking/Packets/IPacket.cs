using Kestrel.Framework.Server;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets;

public enum Packet : byte
{
    C2SPlayerLoginRequest = 1,
    C2SPlayerMove = 2,
    C2SChunkRequest = 3,

    S2CPlayerLoginSuccess = 127,
    S2CBroadcastEntityMove = 128,
    S2CBroadcastEntitySpawn = 129,
    S2CChunkResponse = 130,
}

public interface IPacket
{
    Packet PacketId { get; }
    void Serialize(NetDataWriter writer);
    void Deserialize(NetDataReader reader);

    public static byte[] Serialize(IPacket packet)
    {
        NetDataWriter writer = new();
        writer.Put((byte)packet.PacketId);
        packet.Serialize(writer);
        return writer.Data;
    }
}