namespace Kestrel.Framework.World;

public class Generator
{
    readonly FastNoiseLite fnl1, fnl2;

    public Generator()
    {
        fnl1 = new FastNoiseLite(1337);
        fnl1.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        fnl1.SetFrequency(0.02f);
        fnl1.SetFractalOctaves(4);
        fnl1.SetFractalLacunarity(2.0f);
        fnl1.SetFractalGain(0.5f);

        fnl2 = new FastNoiseLite(1337 * 2);
        fnl2.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        fnl2.SetFrequency(0.008f);
        fnl2.SetFractalOctaves(4);
        fnl2.SetFractalLacunarity(2.0f);
        fnl2.SetFractalGain(0.5f);
    }

    public BlockType GetBlock(int x, int y, int z)
    {
        if (y < 59) return BlockType.Air;
        if (y == 59) return BlockType.Stone;

        float highFrequency = (fnl1.GetNoise(x, z) + 1f) * 0.5f;
        float lowFrequency = (fnl2.GetNoise(x, z) + 1f) * 0.5f;
        float height = highFrequency * 10f + lowFrequency * 50f + 60f;

        int h = (int)MathF.Floor(height);

        if (y > h) return BlockType.Air;
        if (y == h) return BlockType.Grass;
        return BlockType.Stone;
    }
}