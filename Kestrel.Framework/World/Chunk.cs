namespace Kestrel.Framework.World;

public class Chunk
{
    public World World;
    public BlockType[] Blocks = [];
    public int ChunkX, ChunkY, ChunkZ;
    public bool IsEmpty = true;

    public Chunk(World world, int cx, int cy, int cz)
    {
        ChunkX = cx;
        ChunkY = cy;
        ChunkZ = cz;
        World = world;
        Blocks = new BlockType[world.ChunkSize * world.ChunkSize * world.ChunkSize];
    }

    public void Generate()
    {
        IsEmpty = true;
        Parallel.For(0, World.ChunkSize * World.ChunkSize * World.ChunkSize, new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        }, (i) =>
        {
            var (x, y, z) = IndexToChunk(i);
            var (wx, wy, wz) = ChunkToWorld(x, y, z);
            BlockType block = World.Generator.GetBlock(wx, wy, wz);
            Blocks[i] = block;

            if (block != BlockType.Air)
                IsEmpty = false;
        });
    }

    public BlockType? GetBlock(int lx, int ly, int lz)
    {
        if (lx < 0 || lx >= World.ChunkSize ||
            ly < 0 || ly >= World.ChunkSize ||
            lz < 0 || lz >= World.ChunkSize)
        {
            return null;
        }
        return Blocks[ChunkToIndex(lx, ly, lz)];
    }

    public void SetBlock(int lx, int ly, int lz, BlockType block)
    {
        if (lx < 0 || lx >= World.ChunkSize ||
    ly < 0 || ly >= World.ChunkSize ||
    lz < 0 || lz >= World.ChunkSize)
        {
            return;
        }

        Blocks[ChunkToIndex(lx, ly, lz)] = block;
    }

    public int ChunkToIndex(int x, int y, int z)
    {
        return x + y * World.ChunkSize + z * World.ChunkSize * World.ChunkSize;
    }

    public (int lx, int ly, int lz) IndexToChunk(int index)
    {
        int x = index % World.ChunkSize;
        int y = index / World.ChunkSize % World.ChunkSize;
        int z = index / (World.ChunkSize * World.ChunkSize);
        return (x, y, z);
    }

    public (int wx, int wy, int wz) ChunkToWorld(int lx, int ly, int lz)
    {
        return (ChunkX * World.ChunkSize + lx,
                ChunkY * World.ChunkSize + ly,
                ChunkZ * World.ChunkSize + lz);
    }
}