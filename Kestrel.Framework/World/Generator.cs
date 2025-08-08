namespace Kestrel.Framework.World;

public class Generator
{
    readonly FastNoiseLite fnl;

    public Generator()
    {
        fnl = new FastNoiseLite(1337);
        fnl.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        fnl.SetFrequency(0.01f);
        fnl.SetFractalOctaves(4);
        fnl.SetFractalLacunarity(2.0f);
        fnl.SetFractalGain(0.5f);
    }

    public BlockType GetBlock(int x, int y, int z)
    {
        // if (z < 60)
        // {
        //     return BlockType.Stone;
        // }
        // else if (z > 120)
        // {
        //     return BlockType.Stone;
        // }

        // float height = (fnl.GetNoise(x, z) + 1.0f) / 2.0f * 60.0f + 60.0f;
        // if (height < y)
        // {
        //     return BlockType.Stone;
        // }
        // else
        // {
        //     return BlockType.Air;
        // }

        float height = (fnl.GetNoise(x, z) + 1.0f) / 2.0f * 32.0f;
        if (height < y)
        {
            return BlockType.Air;
        }
        else
        {
            return BlockType.Stone;
        }
    }
}