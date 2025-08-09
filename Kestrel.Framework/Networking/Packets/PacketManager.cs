using Kestrel.Framework.Server;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets;

public static class PacketManager
{
    public static byte[] SerializeC2SPacket(IC2SPacket packet)
    {
        NetDataWriter writer = new();
        writer.Put(packet.PacketId);
        packet.Serialize(writer);
        return writer.Data;
    }

    public static byte[] SerializeS2CPacket(IS2CPacket packet)
    {
        NetDataWriter writer = new();
        writer.Put(packet.PacketId);
        packet.Serialize(writer);
        return writer.Data;
    }

    public static IC2SPacket DeserializeC2SPacket(NetDataReader reader)
    {
        ushort packetId = reader.GetUShort();
        if (PacketRegistry.TryGetC2SPacket(packetId, out var c2sPacket))
        {
            c2sPacket!.Deserialize(reader);
            return c2sPacket;
        }
        throw new Exception($"Unknown c2s packet ID: {packetId}");
    }

    public static IS2CPacket DeserializeS2CPacket(NetDataReader reader)
    {
        ushort packetId = reader.GetUShort();
        if (PacketRegistry.TryGetS2CPacket(packetId, out var s2cPacket))
        {
            s2cPacket!.Deserialize(reader);
            return s2cPacket;
        }

        throw new Exception($"Unknown s2c packet ID: {packetId}");
    }

    public static void HandleC2SPacket(IC2SPacket packet, ServerState context, NetPeer client)
    {
        // Console.WriteLine($"Handling C2S packet with ID: {packet.GetType().Name}");
        packet.Handle(context, client);
    }

    public static void HandleS2CPacket(IS2CPacket packet, ClientState context, NetPeer server)
    {
        // Console.WriteLine($"Handling S2C packet with ID: {packet.GetType().Name}");
        packet.Handle(context, server);
    }
}