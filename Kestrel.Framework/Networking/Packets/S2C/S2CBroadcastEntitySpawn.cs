using System.ComponentModel;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using GlmSharp;
using Kestrel.Framework.Entity;
using Kestrel.Framework.Entity.Components;
using LiteNetLib;
using LiteNetLib.Utils;
using ArchEntity = Arch.Core.Entity;

namespace Kestrel.Framework.Networking.Packets.S2C;

public class S2CBroadcastEntitySpawn : IPacket
{
    public Packet PacketId => Packet.S2CBroadcastEntitySpawn;

    public Guid ServerId;
    public INetworkableComponent[] Components = [];

    public void Deserialize(NetDataReader reader)
    {
        ServerId = reader.GetGuid();
        var componentCount = reader.GetInt();

        Components = new INetworkableComponent[componentCount];
        for (int j = 0; j < componentCount; j++)
        {
            INetworkableComponent component = ComponentManager.DeserializeComponent(reader);
            Components[j] = component;
        }
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ServerId);
        writer.Put(Components.Length);
        foreach (var component in Components)
        {
            ComponentManager.SerializeComponent(component, writer);
        }
    }
}