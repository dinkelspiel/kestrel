using System.Numerics;
using System.Text;
using Kestrel.Framework.Networking.Packets.S2C;
using Kestrel.Framework.Server;
using Kestrel.Framework.Server.Player;
using Kestrel.Framework.World;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets.C2S;

public struct C2SChunkRequest : IC2SPacket
{
    public ushort PacketId => 6;
    public int ChunkX, ChunkY, ChunkZ;

    public void Deserialize(NetDataReader reader)
    {
        ChunkX = reader.GetInt();
        ChunkY = reader.GetInt();
        ChunkZ = reader.GetInt();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ChunkX);
        writer.Put(ChunkY);
        writer.Put(ChunkZ);
    }

    public void Handle(ServerState context, NetPeer client)
    {
        Chunk chunk = context.World.GetChunkOrGenerate(ChunkX, ChunkY, ChunkZ);

        client.Send(PacketManager.SerializeS2CPacket(new S2CChunkResponse(chunk)), DeliveryMethod.ReliableUnordered);
    }
}