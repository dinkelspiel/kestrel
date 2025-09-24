using System.Numerics;
using Arch.Core.Extensions;
using GlmSharp;
using Kestrel.Framework.Entity.Components;
using LiteNetLib;
using LiteNetLib.Utils;
using ArchEntity = Arch.Core.Entity;

namespace Kestrel.Framework.Networking.Packets.S2C;

public class S2CBroadcastEntityMove : IPacket
{
    public Packet PacketId => Packet.S2CBroadcastEntityMove;

    public Guid ServerId;
    public Vector3 Position;

    public void Deserialize(NetDataReader reader)
    {
        ServerId = reader.GetGuid();
        Position = new()
        {
            X = reader.GetFloat(),
            Y = reader.GetFloat(),
            Z = reader.GetFloat()
        };
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ServerId);
        writer.Put(Position.X);
        writer.Put(Position.Y);
        writer.Put(Position.Z);
    }
}