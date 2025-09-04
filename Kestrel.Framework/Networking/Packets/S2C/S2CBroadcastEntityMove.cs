using System.Numerics;
using Arch.Core.Extensions;
using GlmSharp;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.Server.Player;
using LiteNetLib;
using LiteNetLib.Utils;
using ArchEntity = Arch.Core.Entity;

namespace Kestrel.Framework.Networking.Packets.S2C;

public class S2CBroadcastEntityMove : IS2CPacket
{
    public ushort PacketId => 4;
    public int ServerId;
    public Vector3 Position;

    public void Deserialize(NetDataReader reader)
    {
        ServerId = reader.GetInt();
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

    public void Handle(ClientState context, NetPeer server)
    {
        // Don't update the position of the player, might want to change this later but yeah
        var playerServerId = context.Player.Get<ServerId>().Id;
        if (ServerId == playerServerId)
        {
            return;
        }

        ArchEntity entity = context.ServerIdToEntity[ServerId];
        entity.Get<Location>().Postion = new Vector3(Position.X, Position.Y, Position.Z);
    }
}