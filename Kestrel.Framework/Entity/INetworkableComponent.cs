namespace Kestrel.Framework.Entity;

using LiteNetLib.Utils;

public interface INetworkableComponent
{
    ushort PacketId { get; }

    void Serialize(NetDataWriter writer);
    void Deserialize(NetDataReader reader);
}