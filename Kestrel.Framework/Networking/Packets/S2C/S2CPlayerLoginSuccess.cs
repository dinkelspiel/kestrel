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

public class S2CPlayerLoginSuccess : IS2CPacket
{
    public ushort PacketId => 2;
    public int EntityCount;
    public Dictionary<int, INetworkableComponent[]> Entities;

    public void Deserialize(NetDataReader reader)
    {
        EntityCount = reader.GetInt();
        Entities = [];
        for (int i = 0; i < EntityCount; i++)
        {
            var componentCount = reader.GetInt();
            var components = new INetworkableComponent[componentCount];
            for (int j = 0; j < componentCount; j++)
            {
                INetworkableComponent component = ComponentManager.DeserializeComponent(reader);
                components[j] = component;
            }
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

    public void Handle(ClientState context, NetPeer server)
    {
        foreach (var entity in Entities)
        {
            ArchEntity archEntity = context.Entities.Create(new ServerId(entity.Key));

            // entity.Key is the server ID so we add it to the dictionary
            context.ServerIdToEntity.TryAdd(entity.Key, archEntity);

            foreach (var component in entity.Value)
            {
                if (component is Player player && player.Name == context.PlayerName)
                {
                    context.Player = archEntity;
                }
                context.Entities.Add(archEntity, component);
            }
        }

        Location position = context.Player.Get<Location>();

        context.World.WorldToChunk((int)position.X, (int)position.Y, (int)position.Z, out var chunkPos, out _);
        position.LastFrameChunkPos = chunkPos;

        foreach (var (x, y, z) in LocationUtil.CoordsNearestFirst(context.RenderDistance, chunkPos.X, chunkPos.Y, chunkPos.Z))
        {
            context.RequestChunk(new(x, y, z));
        }
    }
}