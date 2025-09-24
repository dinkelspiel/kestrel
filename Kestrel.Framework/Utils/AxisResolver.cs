using System.Numerics;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.World;

namespace Kestrel.Framework.Utils;

public static class AxisResolver
{
    public enum Axis { X, Y, Z }

    public static void MoveAxis(World.World world, ref Location pos, ref Velocity vel, ref Collider col, Axis axis, float delta)
    {
        if (delta == 0f) return;

        float sign = MathF.Sign(delta);
        float remaining = MathF.Abs(delta);

        while (remaining > 0f)
        {
            float step = MathF.Min(remaining, 1f);
            Vector3 newPos = pos.Position;

            switch (axis)
            {
                case Axis.X: newPos.X += sign * step; break;
                case Axis.Y: newPos.Y += sign * step; break;
                case Axis.Z: newPos.Z += sign * step; break;
            }

            (var isColliding, var collidingPos) = IsColliding(world, newPos);
            if (isColliding && collidingPos.HasValue)
            {
                switch (axis)
                {
                    case Axis.X:
                        vel.X = 0f; pos.X = collidingPos.Value.X;
                        break;
                    case Axis.Y:
                        if (sign < 0) col.IsOnGround = true;
                        pos.Y = collidingPos.Value.Y;
                        vel.Y = 0f; break;
                    case Axis.Z:
                        vel.Z = 0f; pos.Z = collidingPos.Value.Z;
                        break;
                }
                return;
            }

            pos.Position = newPos;
            remaining -= step;
        }
    }

    public static (bool, Vector3?) IsColliding(World.World world, Vector3 pos)
    {
        int minX = (int)MathF.Floor(pos.X);
        int maxX = (int)MathF.Floor(pos.X);
        int minY = (int)MathF.Floor(pos.Y);
        int maxY = (int)MathF.Floor(pos.Y);
        int minZ = (int)MathF.Floor(pos.Z);
        int maxZ = (int)MathF.Floor(pos.Z);

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    var block = world.GetBlock(x, y, z);
                    if (block.IsSolid())
                        return (true, new(x + 1, y + 1, z + 1));
                }

        return (false, null);
    }

}