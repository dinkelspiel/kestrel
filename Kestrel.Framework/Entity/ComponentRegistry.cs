using Kestrel.Framework.Entity;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.Networking.Packets.C2S;
using Kestrel.Framework.Networking.Packets.S2C;

namespace Kestrel.Framework.Networking.Packets;

public static class ComponentRegistry
{
    public static Dictionary<ushort, INetworkableComponent> Components = [];

    public static void RegisterComponents()
    {
        Register(new Nametag());
        Register(new Location());
        Register(new Player());
        Register(new Velocity());
        Register(new Physics());
        Register(new Collider());
    }

    public static void Register(INetworkableComponent component)
    {
        Components.Add(component.PacketId, component);
    }

    public static bool TryGetComponent(ushort packetId, out INetworkableComponent? component)
    {
        if (Components == null)
        {
            throw new Exception("Components not registered. Call RegisterComponents() first.");
        }
        if (Components.TryGetValue(packetId, out var _component))
        {
            component = _component;
            return true;
        }
        component = null;
        return false;
    }
}