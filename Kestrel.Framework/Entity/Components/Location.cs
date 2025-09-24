using Kestrel.Framework.Utils;
using KestrelWorld = Kestrel.Framework.World.World;
using LiteNetLib.Utils;
using System.Numerics;

namespace Kestrel.Framework.Entity.Components;

public record struct Location : INetworkableComponent
{
    public readonly ushort PacketId => 1;

    public Vector3 Position = new();

    public float X { readonly get { return Position.X; } set { Position.X = value; } }
    public float Y { readonly get { return Position.Y; } set { Position.Y = value; } }
    public float Z { readonly get { return Position.Z; } set { Position.Z = value; } }

    public float LastUpdatedX;
    public float LastUpdatedY;
    public float LastUpdatedZ;

    public Vector3I LastFrameChunkPos;

    public Location(KestrelWorld world, float X, float Y, float Z)
    {
        LastUpdatedX = X;
        LastUpdatedY = Y;
        LastUpdatedZ = Z;

        this.X = X;
        this.Y = Y;
        this.Z = Z;

        world.WorldToChunk((int)X, (int)Y, (int)Z, out Vector3I chunkPos, out _);
        LastFrameChunkPos = chunkPos;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(X);
        writer.Put(Y);
        writer.Put(Z);
    }

    public void Deserialize(NetDataReader reader)
    {
        X = reader.GetFloat();
        Y = reader.GetFloat();
        Z = reader.GetFloat();
    }
};
