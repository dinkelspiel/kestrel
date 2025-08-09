namespace Kestrel.Framework.Utils;

using System.Numerics;
using GlmSharp;

public static class VectorExtensions
{
    public static Vector3 ToVector3(this vec3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static vec3 ToVec3(this Vector3 v)
    {
        return new vec3(v.X, v.Y, v.Z);
    }
}