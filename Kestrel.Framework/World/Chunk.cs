namespace Kestrel.Framework.World;

public class Chunk
{
    public World World;
    public BlockType[] Blocks = [];
    public int ChunkX, ChunkY, ChunkZ;

    public Chunk(World world, int cx, int cy, int cz)
    {
        this.ChunkX = cx;
        this.ChunkY = cy;
        this.ChunkZ = cz;
        this.World = world;
        Blocks = new BlockType[world.ChunkSize * world.ChunkSize * world.ChunkSize];

        for (int i = 0; i < world.ChunkSize * world.ChunkSize * world.ChunkSize; i++)
        {
            var (x, y, z) = IndexToChunk(i);
            var (wx, wy, wz) = ChunkToWorld(x, y, z);
            Blocks[i] = world.Generator.GetBlock(wx, wy, wz);
        }
    }

    public BlockType? GetBlock(int x, int y, int z)
    {
        if (x < 0 || x >= World.ChunkSize ||
            y < 0 || y >= World.ChunkSize ||
            z < 0 || z >= World.ChunkSize)
        {
            return null;
        }
        return Blocks[ChunkToIndex(x, y, z)];
    }

    public int ChunkToIndex(int x, int y, int z)
    {
        return x + y * World.ChunkSize + z * World.ChunkSize * World.ChunkSize;
    }

    public (int x, int y, int z) IndexToChunk(int index)
    {
        int x = index % World.ChunkSize;
        int y = index / World.ChunkSize % World.ChunkSize;
        int z = index / (World.ChunkSize * World.ChunkSize);
        return (x, y, z);
    }

    public (int wx, int wy, int wz) ChunkToWorld(int x, int y, int z)
    {
        return (ChunkX * World.ChunkSize + x,
                ChunkY * World.ChunkSize + y,
                ChunkZ * World.ChunkSize + z);
    }
}