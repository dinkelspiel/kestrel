using System.Formats.Asn1;
using System.Numerics;
using ArchEntity = Arch.Core.Entity;
using GlmSharp;
using Kestrel.Framework.Entity;
using Kestrel.Framework.Networking.Packets.C2S;
using Kestrel.Framework.Server.Player;
using Kestrel.Framework.Utils;
using Kestrel.Framework.World;
using LiteNetLib;
using LiteNetLib.Utils;
using Arch.Core;
using Kestrel.Framework.Entity.Components;
using Arch.Core.Extensions;

namespace Kestrel.Framework.Networking.Packets.S2C;

public class S2CPlayerLoginSuccess : IPacket
{
    public Packet PacketId => Packet.S2CPlayerLoginSuccess;

    public int EntityCount;
    public Dictionary<Guid, INetworkableComponent[]> Entities = [];

    public void Deserialize(NetDataReader reader)
    {
        EntityCount = reader.GetInt();
        for (int i = 0; i < EntityCount; i++)
        {
            var entityId = reader.GetGuid();
            var componentCount = reader.GetInt();
            var components = new INetworkableComponent[componentCount];
            for (int j = 0; j < componentCount; j++)
            {
                INetworkableComponent component = ComponentManager.DeserializeComponent(reader);
                components[j] = component;
            }
            Entities.Add(entityId, components);
        }
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(EntityCount);
        foreach (var entity in Entities)
        {
            writer.Put(entity.Key);
            writer.Put(entity.Value.Length);
            foreach (var component in entity.Value)
            {
                ComponentManager.SerializeComponent(component, writer);
            }
        }
    }
}