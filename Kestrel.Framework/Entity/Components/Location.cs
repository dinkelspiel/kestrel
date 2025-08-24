using LiteNetLib.Utils;

namespace Kestrel.Framework.Entity.Components;

public record struct Location(float X, float Y, float Z) : INetworkableComponent
{
    public readonly ushort PacketId => 1;

    public float LastUpdatedX = X;
    public float LastUpdatedY = Y;
    public float LastUpdatedZ = Z;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(X);
        writer.Put(Y);
        writer.Put(Z);
    }

    public void Deserialize(NetDataReader reader)
    {
        X = reader.GetInt();
        Y = reader.GetInt();
        Z = reader.GetInt();
    }
};
