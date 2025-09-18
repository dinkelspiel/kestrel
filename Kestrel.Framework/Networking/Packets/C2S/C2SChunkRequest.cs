using System.Numerics;
using System.Text;
using Kestrel.Framework.Networking.Packets.S2C;
using Kestrel.Framework.Server;
using Kestrel.Framework.Server.Player;
using Kestrel.Framework.Utils;
using Kestrel.Framework.World;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets.C2S;

public struct C2SChunkRequest : IPacket
{
    public readonly Packet PacketId => Packet.C2SChunkRequest;
    public int ChunkCount;
    public Vector3I[] Chunks;

    public void Deserialize(NetDataReader reader)
    {
        ChunkCount = reader.GetInt();
        Chunks = new Vector3I[ChunkCount];
        for (int i = 0; i < ChunkCount; i++)
        {
            Chunks[i] = new(reader.GetInt(), reader.GetInt(), reader.GetInt());
        }
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ChunkCount);
        for (int i = 0; i < ChunkCount; i++)
        {
            writer.Put(Chunks[i].X);
            writer.Put(Chunks[i].Y);
            writer.Put(Chunks[i].Z);
        }
    }
}