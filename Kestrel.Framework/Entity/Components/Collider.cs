using LiteNetLib.Utils;

namespace Kestrel.Framework.Entity.Components;

public record struct Collider : INetworkableComponent
{
    public readonly ushort PacketId => 6;

    public bool IsOnGround = false;

    public Collider()
    {
    }

    public void Deserialize(NetDataReader reader)
    {
    }

    public void Serialize(NetDataWriter writer)
    {
    }
}
