using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Client.Graphics.Buffers;
using Kestrel.Framework.Utils;
using Kestrel.Framework.World;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets.S2C;

public class PacketChunk
{
    public bool IsEmpty;
    public int ChunkX, ChunkY, ChunkZ;
    public int BlockCount;
    public BlockType[] Blocks;
}

public class S2CChunkResponse : IPacket
{
    public Packet PacketId => Packet.S2CChunkResponse;


    public int ChunkCount;
    public PacketChunk[] Chunks;

    public S2CChunkResponse()
    {
    }

    public S2CChunkResponse(Chunk[] chunks)
    {
        int index = -1;
        Chunks = new PacketChunk[chunks.Length];
        ChunkCount = chunks.Length;
        foreach (var chunk in chunks)
        {
            index++;
            PacketChunk packetChunk = new()
            {
                ChunkX = chunk.ChunkX,
                ChunkY = chunk.ChunkY,
                ChunkZ = chunk.ChunkZ,
                IsEmpty = chunk.IsEmpty,
                BlockCount = chunk.Blocks.Length,
                Blocks = chunk.Blocks,
            };
            Chunks[index] = packetChunk;
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        ChunkCount = reader.GetInt();
        Chunks = new PacketChunk[ChunkCount];
        for (int i = 0; i < ChunkCount; i++)
        {
            PacketChunk packetChunk = new()
            {
                IsEmpty = reader.GetBool(),
                ChunkX = reader.GetInt(),
                ChunkY = reader.GetInt(),
                ChunkZ = reader.GetInt(),
                BlockCount = reader.GetInt()
            };
            packetChunk.Blocks = new BlockType[packetChunk.BlockCount];
            if (!packetChunk.IsEmpty)
            {
                for (int j = 0; j < packetChunk.BlockCount; j++)
                {
                    packetChunk.Blocks[j] = (BlockType)reader.GetInt();
                }
            }
            Chunks[i] = packetChunk;
        }
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ChunkCount);
        for (int i = 0; i < ChunkCount; i++)
        {
            PacketChunk chunk = Chunks[i];
            writer.Put(chunk.IsEmpty);
            writer.Put(chunk.ChunkX);
            writer.Put(chunk.ChunkY);
            writer.Put(chunk.ChunkZ);
            writer.Put(chunk.BlockCount);
            if (!chunk.IsEmpty)
            {
                foreach (var block in chunk.Blocks)
                {
                    writer.Put((int)block);
                }
            }
        }
    }
}