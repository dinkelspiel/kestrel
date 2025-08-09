using System.Numerics;

namespace Kestrel.Framework.Utils;

public readonly struct Vector3I : IEquatable<Vector3I>
{
    public readonly int X, Y, Z;
    public Vector3I(int x, int y, int z) { X = x; Y = y; Z = z; }

    public bool Equals(Vector3I other) => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object? obj) => obj is Vector3I vi && Equals(vi);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    public Vector3 ToVector3()
    {
        return new(X, Y, Z);
    }
}