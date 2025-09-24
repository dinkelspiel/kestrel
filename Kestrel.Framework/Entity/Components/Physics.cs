using LiteNetLib.Utils;

namespace Kestrel.Framework.Entity.Components;

public record struct Physics : INetworkableComponent
{
    public readonly ushort PacketId => 5;

    // 9.98 / 20, 20 = ticks/seconds, 9.92 = Gravity
    public const float GRAVITY = -0.491f;

    public void Deserialize(NetDataReader reader)
    {
    }

    public void Serialize(NetDataWriter writer)
    {
    }
}
