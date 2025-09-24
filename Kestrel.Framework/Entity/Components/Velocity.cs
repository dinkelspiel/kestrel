namespace Kestrel.Framework.Entity.Components;

using System.Numerics;
using LiteNetLib.Utils;

public record struct Velocity : INetworkableComponent
{
    public readonly ushort PacketId => 2;

    public Vector3 Vel = new();
    public Vector3 WishVel = new();

    public float X { readonly get { return Vel.X; } set { Vel.X = value; } }
    public float Y { readonly get { return Vel.Y; } set { Vel.Y = value; } }
    public float Z { readonly get { return Vel.Z; } set { Vel.Z = value; } }


    public float DistanceMoved = 0;

    public Velocity(float X, float Y, float Z)
    {
        Vel.X = X;
        Vel.Y = Y;
        Vel.Z = Z;
    }

    public void Deserialize(NetDataReader reader)
    {
        Vel.X = reader.GetFloat();
        Vel.Y = reader.GetFloat();
        Vel.Z = reader.GetFloat();

        WishVel.X = reader.GetFloat();
        WishVel.Y = reader.GetFloat();
        WishVel.Z = reader.GetFloat();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Vel.X);
        writer.Put(Vel.Y);
        writer.Put(Vel.Z);

        writer.Put(WishVel.X);
        writer.Put(WishVel.Y);
        writer.Put(WishVel.Z);
    }
}
