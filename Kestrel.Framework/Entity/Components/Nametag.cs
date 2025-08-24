using LiteNetLib.Utils;

namespace Kestrel.Framework.Entity.Components;

public record struct Nametag(string Name) : INetworkableComponent
{
    public readonly ushort PacketId => 3;

    public void Deserialize(NetDataReader reader)
    {
        Name = reader.GetString(64);
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Name, 64);
    }
}
