using Kestrel.Framework.Entity;
using Kestrel.Framework.Server;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets;

public static class ComponentManager
{
    public static void SerializeComponent(INetworkableComponent component, NetDataWriter writer)
    {
        writer.Put(component.PacketId);
        component.Serialize(writer);
    }

    public static INetworkableComponent DeserializeComponent(NetDataReader reader)
    {
        ushort packetId = reader.GetUShort();
        if (ComponentRegistry.TryGetComponent(packetId, out var component))
        {
            component!.Deserialize(reader);
            return component;
        }
        throw new Exception($"Unknown component ID: {packetId}");
    }
}