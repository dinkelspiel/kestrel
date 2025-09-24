namespace Kestrel.Framework.Client.Graphics;

using System.Numerics;
using Arch.Core.Extensions;
using GlmSharp;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.Utils;
using Silk.NET.Input;

public class ThirdPersonCamera : Camera.Camera
{
    ClientState clientState;

    // Rendering basis
    public vec3 front = new(0.0f, 0.0f, -1.0f);
    public vec3 up = new(0.0f, 1.0f, 0.0f);

    // --- Third-person orbit state ---
    public float OrbitDistance = 50.0f;
    public float MinDistance = 2.0f;
    public float MaxDistance = 12.0f;

    public float ShoulderHeight = 1.6f; // where on the player we look
    public float MinPitch = -1.2f;      // ~ -69°
    public float MaxPitch = 1.2f;      // ~  69°

    // Movement basis (for WASD)
    public vec3 moveForward = new(0, 0, -1);
    public vec3 moveRight = new(1, 0, 0);

    // Internal look state
    float lastX = 400, lastY = 300;
    float yaw = -90.0f;   // degrees
    float pitch = 15.0f;  // a bit downward by default

    public ThirdPersonCamera(ClientState clientState) : base(clientState)
    {
        this.clientState = clientState;
        RebuildMoveBasis();
    }

    public new mat4 View
    {
        get
        {
            // point we want to look at (player “head”)
            var target = clientState.Player.Get<Location>().Position.ToVec3() + new vec3(0, ShoulderHeight, 0);

            // spherical orbit offset from yaw/pitch/distance
            float yawR = glm.Radians(yaw);
            float pitchR = glm.Radians(pitch);

            var offset = new vec3(
                OrbitDistance * MathF.Cos(pitchR) * MathF.Sin(yawR),
                OrbitDistance * MathF.Sin(pitchR),
                OrbitDistance * MathF.Cos(pitchR) * MathF.Cos(yawR)
            );

            var eye = target - offset;

            // update render-facing vectors
            front = glm.Normalized(target - eye);
            up = new vec3(0, 1, 0);

            return mat4.LookAt(eye, target, up);
        }
    }

    // Mouse move now orbits around the player instead of rotating the player “eyes”
    public new void OnMouseMove(IMouse mouse, Vector2 position)
    {
        float xoffset = position.X - lastX;
        float yoffset = lastY - position.Y; // reversed since y up
        lastX = position.X;
        lastY = position.Y;

        const float sensitivity = 0.1f;
        yaw += xoffset * sensitivity;
        pitch += yoffset * sensitivity;

        // clamp pitch to avoid flipping over the top/bottom
        pitch = Math.Clamp(pitch, glm.Degrees(MinPitch), glm.Degrees(MaxPitch));

        RebuildMoveBasis();
    }

    // Add this: scroll wheel zoom for third-person
    public void OnScroll(IMouse mouse, ScrollWheel scroll)
    {
        OrbitDistance = Math.Clamp(OrbitDistance - scroll.Y, MinDistance, MaxDistance);
    }

    // Build a flat movement basis so WASD is camera-relative but not “into the camera”
    private void RebuildMoveBasis()
    {
        float yawR = glm.Radians(yaw);

        // forward on the XZ plane from yaw (ignores pitch so you don’t fly when pressing W)
        moveForward = glm.Normalized(new vec3(MathF.Sin(yawR), 0, MathF.Cos(yawR)));
        moveRight = glm.Normalized(glm.Cross(moveForward, new vec3(0, 1, 0)));
    }
}
