using System.Numerics;
using System.Text;
using Arch.Core;
using Arch.Core.Extensions;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.Networking.Packets.S2C;
using Kestrel.Framework.Server;
using Kestrel.Framework.Server.Player;
using LiteNetLib;
using LiteNetLib.Utils;
using ArchEntity = Arch.Core.Entity;

namespace Kestrel.Framework.Networking.Packets.C2S;

public struct C2SPlayerMove(Vector3 location) : IC2SPacket
{
    public Vector3 Location = location;

    public readonly ushort PacketId => 3;

    public void Deserialize(NetDataReader reader)
    {
        Location = new()
        {
            X = reader.GetFloat(),
            Y = reader.GetFloat(),
            Z = reader.GetFloat()
        };
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Location.X);
        writer.Put(Location.Y);
        writer.Put(Location.Z);
    }

    public void Handle(ServerState context, NetPeer client)
    {
        if (!context.PlayersByConnection.TryGetValue(client, out ArchEntity player))
        {
            Console.WriteLine($"Client not found in context.");
            return;
        }
        player.Get<Location>().Postion = Location;
        var playerServerId = player.Get<ServerId>().Id;

        context.NetServer.SendToAll(PacketManager.SerializeS2CPacket(new S2CBroadcastEntityMove()
        {
            ServerId = playerServerId,
            Position = new Vector3(Location.X, Location.Y, Location.Z)
        }), DeliveryMethod.ReliableOrdered);
    }
}