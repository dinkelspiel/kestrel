using System.Numerics;

namespace Kestrel.Client.ECS;

public struct TransformComponent
{
    public Vector3 Postition;
    public float Yaw;
    public float Pitch;
}

public struct VelocityComponent
{
    public Vector3 Velocity;
}

public struct PlayerTag;