namespace Kestrel.Framework.Entity;

using LiteNetLib.Utils;

public interface INetworkableComponent
{
    int PacketId { get; }

    void Serialize(NetDataWriter writer);
    void Deserialize(NetDataReader reader);
}