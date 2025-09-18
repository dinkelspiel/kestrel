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
        Console.WriteLine("Eligble Components:");
        foreach (var _component in ComponentRegistry.Components.ToList())
        {
            Console.Write("{0}: {1}, ", _component.Key, _component.Value.GetType());
        }
        Console.WriteLine();
        throw new Exception($"Unknown component ID: {packetId}");
    }
}