using LiteNetLib.Utils;

namespace Kestrel.Framework.Entity.Components;

public record struct Player(string Name) : INetworkableComponent
{
    public readonly ushort PacketId => 4;

    public void Deserialize(NetDataReader reader)
    {
        Name = reader.GetString(64);
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Name, 64);
    }
}
