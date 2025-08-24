namespace Kestrel.Framework.Entity.Components;

using LiteNetLib.Utils;

public record struct Velocity(float X, float Y, float Z) : INetworkableComponent
{
    public readonly ushort PacketId => 2;

    public void Deserialize(NetDataReader reader)
    {
        X = reader.GetFloat();
        Y = reader.GetFloat();
        Z = reader.GetFloat();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(X);
        writer.Put(Y);
        writer.Put(Z);
    }
}
