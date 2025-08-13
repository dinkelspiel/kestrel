using System.Collections.Concurrent;
using Kestrel.Framework.World.Structures;

namespace Kestrel.Framework.World;

public class Generator
{
    readonly FastNoiseLite fnl1, fnl2, mountainness, ncliffness, nelevation, worley, nvariance;
    readonly World world;
    readonly ConcurrentDictionary<(int x, int y, int z), BlockType> structureBlockStore = new();
    readonly HashSet<(int x, int y, int z)> structures = new();

    public Generator(World world)
    {
        this.world = world;

        fnl1 = new FastNoiseLite(1337);
        fnl1.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        fnl1.SetFractalType(FastNoiseLite.FractalType.FBm);
        fnl1.SetFrequency(0.02f);
        fnl1.SetFractalOctaves(4);
        fnl1.SetFractalLacunarity(2.0f);
        fnl1.SetFractalGain(0.5f);

        fnl2 = new FastNoiseLite(1337 * 2);
        fnl2.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        fnl2.SetFractalType(FastNoiseLite.FractalType.FBm);
        fnl2.SetFrequency(0.008f);
        fnl2.SetFractalOctaves(4);
        fnl2.SetFractalLacunarity(2.0f);
        fnl2.SetFractalGain(0.5f);

        mountainness = new FastNoiseLite(1337 * 3);
        mountainness.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        mountainness.SetFrequency(0.004f);
        mountainness.SetFractalOctaves(4);
        mountainness.SetFractalLacunarity(2.0f);
        mountainness.SetFractalGain(0.5f);

        ncliffness = new FastNoiseLite(1337 * 4);
        ncliffness.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        ncliffness.SetFrequency(0.02f);
        ncliffness.SetFractalOctaves(4);
        ncliffness.SetFractalLacunarity(2.0f);
        ncliffness.SetFractalGain(0.5f);

        nelevation = new FastNoiseLite(1337 * 5);
        nelevation.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        nelevation.SetFrequency(0.001f);
        nelevation.SetFractalOctaves(4);
        nelevation.SetFractalLacunarity(2.0f);
        nelevation.SetFractalGain(0.5f);

        worley = new FastNoiseLite(1337);
        worley.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        worley.SetFrequency(0.04f);
        worley.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
        worley.SetCellularJitter(1);

        nvariance = new FastNoiseLite(1337 * 7);
        nvariance.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        nvariance.SetFrequency(0.05f);
        nvariance.SetFractalOctaves(4);
        nvariance.SetFractalLacunarity(2.0f);
        nvariance.SetFractalGain(0.5f);
    }

    public float GetClosestStructure(int wx, int wy, int wz)
    {
        double minDistance = double.MaxValue;

        foreach (var (x, y, z) in structures)
        {
            double dx = x - wx;
            double dy = y - wy;
            double dz = z - wz;

            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }
        return (float)minDistance;
    }

    public float GetMountainness(float x, float z)
    {
        var fx = (mountainness.GetNoise(x, z) + 1.0f) / 2.0f;
        return 1.042f * (float)Math.Pow(fx, 4) + 5.625f * (float)Math.Pow(fx, 3) - 7.542f * (float)Math.Pow(fx, 2) + 3.275f * fx - 0.4f;
    }

    public float SignedDistanceToCoast(float x, float z)
    {
        float f = nelevation.GetNoise(x, z);

        const float d = 1f;

        float fx = (nelevation.GetNoise(x + d, z) - nelevation.GetNoise(x - d, z)) / (2f * d);
        float fz = (nelevation.GetNoise(x, z + d) - nelevation.GetNoise(x, z - d)) / (2f * d);

        float grad = MathF.Sqrt(fx * fx + fz * fz);

        grad = MathF.Max(grad, 1e-4f);

        return f / grad;
    }

    public void SpawnStructure(int wx, int wy, int wz, Structure structure)
    {
        var blocks = structure.GetBlocks();

        for (int ly = 0; ly < blocks.GetLength(0); ly++)
        {
            int rowLength = blocks.GetLength(1);

            for (int i = 0; i < rowLength; i++)
            {
                int lx = i % structure.GetWidth();
                int lz = i / structure.GetWidth();

                BlockType block = blocks[ly, i];

                if (world.LocationIsLoaded(wx + lx, wy + ly, wz + lz, out var chunk))
                {
                    chunk.SetBlock(lx, ly, lz, block);
                    continue;
                }

                structureBlockStore.TryAdd((wx + lx, wy + ly, wz + lz), block);
            }
        }
    }

    public BlockType GetBlock(int wx, int wy, int wz)
    {
        // if (y < 59) return BlockType.Air;
        // if (y == 59) return BlockType.Stone;

        float highFrequency = (fnl1.GetNoise(wx, wz) + 1f) * 0.5f;
        float lowFrequency = (fnl2.GetNoise(wx, wz) + 1f) * 0.5f;
        float mountainess = GetMountainness(wx, wz);
        float elevation = nelevation.GetNoise(wx, wz);
        float cliffness = (ncliffness.GetNoise(wx, wz) + 1f) * 0.5f;

        World.WorldToChunk(wx, wy, wz, world.ChunkSize, out var chunkPos, out var chunkLocal);
        float height = (highFrequency * 10f + lowFrequency * 50f) * mountainess + 60f;

        if (cliffness > 0.8f)
        {
            height += (9 + nvariance.GetNoise(wx, wz) * 4.0f) * mountainess;
        }

        int h = (int)MathF.Floor(height);

        // Seaside cliffs
        float cliffVarianceOffset = nvariance.GetNoise(wx, wz) * 20.0f;
        float maxSeaCliffDistance = 15 + cliffVarianceOffset; // band width into sea
        float distanceToCoast = SignedDistanceToCoast(wx, wz); // > 0 land, < 0 sea

        if (distanceToCoast <= 0 && -distanceToCoast < maxSeaCliffDistance)
        {
            float seaLevel = 59f;
            float heightOverSea = h - seaLevel;

            float t = -distanceToCoast / maxSeaCliffDistance;

            float e = (float)Math.Pow(2f, 10f * t - 10);
            float w = 1f - e;

            float appliedHeight = seaLevel + heightOverSea * w;

            if (wy == Math.Floor(seaLevel)) return BlockType.Water;
            else if (wy == MathF.Floor(appliedHeight) && mountainess < 0.25) return BlockType.Grass;
            else if (wy == MathF.Floor(appliedHeight) + 1 && -distanceToCoast < 9f + cliffVarianceOffset) return BlockType.Grass;
            else if (wy < appliedHeight) return BlockType.Stone;
            else return BlockType.Air;
        }

        if (elevation < 0f && wy == 59.0f)
        {
            return BlockType.Water;
        }
        else if (elevation < 0f && wy < 60.0f + 60.0f * elevation)
        {
            return BlockType.Stone;
        }
        else if (elevation < 0f && wy < 60)
        {
            return BlockType.Water;
        }

        // Coastal Pillars
        else if (mountainess > 0.15f && elevation < 0.0f && elevation > -0.18f)
        {
            float d = (float)worley.GetNoise(wx, wz) + (nvariance.GetNoise(wx, wy) / 2.0f + nvariance.GetNoise(wz, wy) / 2.0f) / 20.0f;
            if (d < -0.98f)
            {
                if (wy < h) return BlockType.Stone;
                else if (wy == h) return BlockType.Grass;
            }
            return BlockType.Air;
        }
        else if (elevation < 0f)
        {
            return BlockType.Air;
        }

        if (mountainess > 0.8f && wy == h + 1 && GetClosestStructure(wx, wy, wz) > 1000.0f)
        {
            SpawnStructure(wx, wy, wz, new StructureChurch());
            structures.Add((wx, wy, wz));
        }

        if (structureBlockStore.ContainsKey((wx, wy, wz)))
        {
            return structureBlockStore[(wx, wy, wz)];
        }

        // Normal hilly
        if (wy == h + 1 && nvariance.GetNoise(wx, wz) < -0.6f && elevation < 0.4f) return BlockType.Leaves;
        if (wy == h + 2 && nvariance.GetNoise(wx, wz) < -0.7f && elevation < 0.35f) return BlockType.Leaves;
        if (wy == h + 3 && nvariance.GetNoise(wx, wz) < -0.8f && elevation < 0.3f) return BlockType.Leaves;
        if (wy > h) return BlockType.Air;
        if (wy == h) return BlockType.Grass;
        return BlockType.Stone;
    }
}