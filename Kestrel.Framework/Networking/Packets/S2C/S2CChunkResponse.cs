using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Client.Graphics.Buffers;
using Kestrel.Framework.Server.Player;
using Kestrel.Framework.Utils;
using Kestrel.Framework.World;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets.S2C;

public class S2CChunkResponse : IS2CPacket
{
    public ushort PacketId => 6;
    public bool IsEmpty;
    public int ChunkX, ChunkY, ChunkZ;
    public int BlockCount;
    public BlockType[] Blocks;

    public S2CChunkResponse()
    {
    }

    public S2CChunkResponse(Chunk chunk)
    {
        ChunkX = chunk.ChunkX;
        ChunkY = chunk.ChunkY;
        ChunkZ = chunk.ChunkZ;
        IsEmpty = chunk.IsEmpty;
        BlockCount = chunk.Blocks.Length;
        Blocks = chunk.Blocks;
    }

    public void Deserialize(NetDataReader reader)
    {
        IsEmpty = reader.GetBool();
        ChunkX = reader.GetInt();
        ChunkY = reader.GetInt();
        ChunkZ = reader.GetInt();
        BlockCount = reader.GetInt();
        Blocks = new BlockType[BlockCount];
        if (!IsEmpty)
        {
            for (int i = 0; i < BlockCount; i++)
            {
                Blocks[i] = (BlockType)reader.GetInt();
            }
        }
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(IsEmpty);
        writer.Put(ChunkX);
        writer.Put(ChunkY);
        writer.Put(ChunkZ);
        writer.Put(BlockCount);
        if (!IsEmpty)
        {
            foreach (var block in Blocks)
            {
                writer.Put((int)block);
            }
        }
    }

    public void Handle(ClientState context, NetPeer server)
    {
        var chunk = new Chunk(context.World, ChunkX, ChunkY, ChunkZ) { Blocks = Blocks, IsEmpty = IsEmpty };
        context.World.SetChunk(ChunkX, ChunkY, ChunkZ, chunk);

        var key = new Vector3I(ChunkX, ChunkY, ChunkZ);
        context.ChunkMeshes.Remove(key);

        var mesh = new ChunkMesh(context, chunk);
        mesh.Generate();
        context.ChunkMeshes.Add(key, mesh);

        if (context.ChunkMeshes.TryGetValue(new Vector3I(ChunkX, ChunkY + 1, ChunkZ), out var topMesh)) topMesh.Generate();
        if (context.ChunkMeshes.TryGetValue(new Vector3I(ChunkX, ChunkY - 1, ChunkZ), out var bottomMesh)) bottomMesh.Generate();
        if (context.ChunkMeshes.TryGetValue(new Vector3I(ChunkX, ChunkY, ChunkZ + 1), out var northMesh)) northMesh.Generate();
        if (context.ChunkMeshes.TryGetValue(new Vector3I(ChunkX, ChunkY, ChunkZ - 1), out var southMesh)) southMesh.Generate();
        if (context.ChunkMeshes.TryGetValue(new Vector3I(ChunkX - 1, ChunkY, ChunkZ), out var westMesh)) westMesh.Generate();
        if (context.ChunkMeshes.TryGetValue(new Vector3I(ChunkX + 1, ChunkY, ChunkZ), out var eastMesh)) eastMesh.Generate();
    }
}