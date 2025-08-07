using System.Numerics;
using System.Text;
using Kestrel.Framework.Networking.Packets.S2C;
using Kestrel.Framework.Server;
using Kestrel.Framework.Server.Player;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets.C2S;

public struct C2SPlayerMove(Vector3 location) : IC2SPacket
{
    public Vector3 Location = location;

    public readonly ushort PacketId => 3;

    public void Deserialize(NetDataReader reader)
    {
        Location = new();
        Location.X = reader.GetFloat();
        Location.Y = reader.GetFloat();
        Location.Z = reader.GetFloat();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Location.X);
        writer.Put(Location.Y);
        writer.Put(Location.Z);
    }

    public void Handle(ServerState context, NetPeer client)
    {
        ServerPlayer player = context.PlayersByConnection[client];
        if (client == null)
        {
            Console.WriteLine($"Client not found in context.");
            return;
        }

        player.Location = new(Location.X, Location.Y, Location.Z);


        context.NetServer.SendToAll(PacketManager.SerializeS2CPacket(new S2CBroadcastPlayerMove()
        {
            PlayerName = player.Name,
            Position = new Vector3(player.Location.x, player.Location.y, player.Location.z)
        }), DeliveryMethod.ReliableOrdered);
    }
}