using Kestrel.Framework.Networking.Packets.C2S;
using Kestrel.Framework.Networking.Packets.S2C;

namespace Kestrel.Framework.Networking.Packets;

public static class PacketRegistry
{
    private static Dictionary<ushort, IC2SPacket> c2sPackets = [];
    private static Dictionary<ushort, IS2CPacket> s2cPackets = [];

    public static void RegisterPackets()
    {
        RegisterS2C(new S2CPlayerLoginSuccess());
        RegisterS2C(new S2CBroadcastPlayerMove());
        RegisterS2C(new S2CBroadcastPlayerJoin());

        RegisterC2S(new C2SPlayerLoginRequest());
        RegisterC2S(new C2SPlayerMove());
    }

    public static void RegisterS2C(IS2CPacket packet)
    {
        s2cPackets!.Add(packet.PacketId, packet);
    }

    public static void RegisterC2S(IC2SPacket packet)
    {
        c2sPackets!.Add(packet.PacketId, packet);
    }

    public static bool TryGetC2SPacket(ushort packetId, out IC2SPacket? packet)
    {
        if (c2sPackets == null)
        {
            throw new Exception("C2S packets not registered. Call RegisterPackets() first.");
        }
        if (c2sPackets.TryGetValue(packetId, out var _packet))
        {
            packet = _packet;
            return true;
        }
        packet = null;
        return false;
    }

    public static bool TryGetS2CPacket(ushort packetId, out IS2CPacket? packet)
    {
        if (s2cPackets == null)
        {
            throw new Exception("S2C packets not registered. Call RegisterPackets() first.");
        }
        if (s2cPackets.TryGetValue(packetId, out var _packet))
        {
            packet = _packet;
            return true;
        }
        packet = null;
        return false;
    }
}