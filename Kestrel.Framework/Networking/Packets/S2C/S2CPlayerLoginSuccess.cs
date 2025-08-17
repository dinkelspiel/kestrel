using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Entity;
using Kestrel.Framework.Networking.Packets.C2S;
using Kestrel.Framework.Server.Player;
using Kestrel.Framework.Utils;
using Kestrel.Framework.World;
using LiteNetLib;
using LiteNetLib.Utils;

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
        }
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Position.X);
        writer.Put(Position.Y);
        writer.Put(Position.Z);
        writer.Put(PlayerCount);
        foreach (var player in Players)
        {
            writer.Put(player.Name, 64);
            writer.Put(player.Location.X);
            writer.Put(player.Location.Y);
            writer.Put(player.Location.Z);
        }
    }

    public void Handle(ClientState context, NetPeer server)
    {
        context.Player.Location = new Vector3(Position.X, Position.Y, Position.Z);

        context.World.WorldToChunk((int)Position.X, (int)Position.Y, (int)Position.Z, out var chunkPos, out _);
        context.Player.LastFrameChunkPos = chunkPos;

        foreach (var player in Players)
        {
            if (player.Name == context.Player.Name)
            {
                continue; // Skip self
            }

            context.Players.TryAdd(player.Name, new ClientPlayer
            {
                Name = player.Name,
                Location = player.Location
            });
        }

        foreach (var (x, y, z) in LocationUtil.CoordsNearestFirst(context.RenderDistance, chunkPos.X, chunkPos.Y, chunkPos.Z))
        {
            context.RequestChunk(new(x, y, z));
        }
    }
}