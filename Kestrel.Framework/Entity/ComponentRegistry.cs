using Kestrel.Framework.Entity;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.Networking.Packets.C2S;
using Kestrel.Framework.Networking.Packets.S2C;

namespace Kestrel.Framework.Networking.Packets;

public static class ComponentRegistry
{
    private static Dictionary<ushort, INetworkableComponent> components = [];

    public static void RegisterComponents()
    {
        Register(new DisplayName());
        Register(new Location());
        Register(new Player());
        Register(new Velocity());
    }

    public static void Register(INetworkableComponent component)
    {
        components.Add(component.PacketId, component);
    }

    public static bool TryGetComponent(ushort packetId, out INetworkableComponent? component)
    {
        if (components == null)
        {
            throw new Exception("Components not registered. Call RegisterComponents() first.");
        }
        if (components.TryGetValue(packetId, out var _component))
        {
            component = _component;
            return true;
        }
        component = null;
        return false;
    }
}