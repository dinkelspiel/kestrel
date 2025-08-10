using System.Numerics;

namespace Kestrel.Framework.Utils;

public static class LocationUtil
{
    public static IEnumerable<(int x, int y, int z)> CoordsNearestFirst(int radius, int cx, int cy, int cz)
    {
        int ry = Math.Max(0, radius / 2);                // half height in Y
        int r2 = radius * radius;                        // XZ radius^2 (euclidean)

        var list = new List<(int dx, int dy, int dz)>();
        for (int dx = -radius; dx <= radius; dx++)
            for (int dz = -radius; dz <= radius; dz++)
            {
                // keep points inside the XZ circle of 'radius'
                if (dx * dx + dz * dz > r2) continue;

                for (int dy = -ry; dy <= ry; dy++)
                    list.Add((dx, dy, dz));
            }

        list.Sort((a, b) =>
        {
            // 1) prioritize by XZ distance (rings)
            int ra = a.dx * a.dx + a.dz * a.dz;
            int rb = b.dx * b.dx + b.dz * b.dz;
            if (ra != rb) return ra.CompareTo(rb);

            // 2) then by vertical distance from the player (|dy|)
            int ady = Math.Abs(a.dy);
            int bdy = Math.Abs(b.dy);
            if (ady != bdy) return ady.CompareTo(bdy);

            // 3) for equal |dy|, prefer +dy over -dy (0, +1, -1, +2, -2, ...)
            if (a.dy != b.dy)
            {
                int sa = a.dy >= 0 ? a.dy : -a.dy - 1;
                int sb = b.dy >= 0 ? b.dy : -b.dy - 1;
                if (sa != sb) return sa.CompareTo(sb);
            }

            // 4) final tie-breaker: spread directions in XZ using Morton/Z-order
            ulong ma = Morton2D(a.dx & 0x7FF, a.dz & 0x7FF);
            ulong mb = Morton2D(b.dx & 0x7FF, b.dz & 0x7FF);
            return ma.CompareTo(mb);
        });

        foreach (var (dx, dy, dz) in list)
            yield return (cx + dx, cy + dy, cz + dz);
    }

    // 2D Morton (bit interleave) for tie-breaking in XZ
    static ulong Morton2D(int x, int z)
    {
        static ulong Part(ulong v)
        {
            v = (v | (v << 32)) & 0x1F00000000FFFF;
            v = (v | (v << 16)) & 0x1F0000FF0000FF;
            v = (v | (v << 8)) & 0x100F00F00F00F00F;
            v = (v | (v << 4)) & 0x10C30C30C30C30C3;
            v = (v | (v << 2)) & 0x1249249249249249;
            return v;
        }
        unchecked
        {
            return Part((ulong)x) | (Part((ulong)z) << 1);
        }
    }

    public static float Distance(Vector3 a, Vector3 b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        float dz = a.Z - b.Z;

        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static float HorizontallyWeightedDistance(Vector3 a, Vector3 b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        float dz = a.Z - b.Z;

        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}