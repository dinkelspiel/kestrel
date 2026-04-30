using System.Numerics;
using Kestrel.Client.Mesh;

namespace Kestrel.Client.ECS;

public struct TransformComponent(Vector3 position)
{
    public Vector3 Postition = position;
    public float Yaw = 0;
    public float Pitch = 0;
    public bool IsGrounded = false;
}

public struct VelocityComponent
{
    public Vector3 Velocity;
}

public struct PlayerTag;

public struct HeightmapColliderComponent;

public struct ModelRendererComponent(ModelDrawInstruction model)
{
    public ModelDrawInstruction Model = model;
}