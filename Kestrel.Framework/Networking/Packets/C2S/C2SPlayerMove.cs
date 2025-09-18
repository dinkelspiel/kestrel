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

public struct C2SPlayerMove(Vector3 location) : IPacket
{
    public readonly Packet PacketId => Packet.C2SPlayerMove;

    public Vector3 Location = location;

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
}