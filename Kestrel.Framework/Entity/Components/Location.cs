using Kestrel.Framework.Utils;
using KestrelWorld = Kestrel.Framework.World.World;
using LiteNetLib.Utils;
using System.Numerics;

namespace Kestrel.Framework.Entity.Components;

public record struct Location : INetworkableComponent
{
    public readonly ushort PacketId => 1;

    public Vector3 Postion
    {
        get => new(X, Y, Z);
        set
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
        }
    }

    public float X;
    public float Y;
    public float Z;

    public float LastUpdatedX;
    public float LastUpdatedY;
    public float LastUpdatedZ;

    public Vector3I LastFrameChunkPos;

    public Location(KestrelWorld world, float X, float Y, float Z)
    {
        LastUpdatedX = X;
        LastUpdatedY = Y;
        LastUpdatedZ = Z;

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
        X = reader.GetInt();
        Y = reader.GetInt();
        Z = reader.GetInt();
    }
};
