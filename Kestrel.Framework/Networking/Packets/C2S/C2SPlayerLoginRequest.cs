using System.Numerics;
using System.Text;
using GlmSharp;
using Kestrel.Framework.Entity;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.Networking.Packets.S2C;
using Kestrel.Framework.Server;
using LiteNetLib;
using LiteNetLib.Utils;
using ArchEntity = Arch.Core.Entity;

namespace Kestrel.Framework.Networking.Packets.C2S;

public struct C2SPlayerLoginRequest(string playerName) : IPacket
{
    public readonly Packet PacketId => Packet.C2SPlayerLoginRequest;

    public string PlayerName = playerName;

    public void Deserialize(NetDataReader reader)
    {
        // Console.WriteLine($"offset: {reader.UserDataOffset} {reader.AvailableBytes}");
        // byte[] guidBytes = new byte[16];
        // reader.GetBytes(guidBytes, 16);
        // playerGuid = new Guid(guidBytes);
        PlayerName = reader.GetString(64);
    }

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(PlayerName, 64);
    }
}