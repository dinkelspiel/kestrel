using LiteNetLib.Utils;

namespace Kestrel.Framework.Entity.Components;

public record struct DisplayName(string Name) : INetworkableComponent
{
    public readonly int PacketId => 3;

    public void Deserialize(NetDataReader reader)
    {
        Name = reader.GetString(64);
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Name, 64);
    }
}
