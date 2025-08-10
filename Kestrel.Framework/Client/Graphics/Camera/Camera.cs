namespace Kestrel.Framework.Client.Graphics.Camera;

using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Utils;
using Silk.NET.Input;
using Silk.NET.Vulkan;

public abstract class Camera
{
    ClientState clientState;
    public vec3 front = new(0.0f, 0.0f, -1.0f);
    public vec3 up = new(0.0f, 1.0f, 0.0f);

    vec3 direction;
    float lastX = 400, lastY = 300;
    float yaw = -90.0f, pitch = 0.0f;

    public Camera(ClientState clientState)
    {
        this.clientState = clientState;

        direction.x = MathF.Cos(glm.Radians(yaw));
        direction.y = MathF.Sin(glm.Radians(pitch));
        direction.z = MathF.Sin(glm.Radians(yaw));
    }

    public mat4 View
    {
        get
        {
            return mat4.LookAt(clientState.Player.Location.ToVec3(), clientState.Player.Location.ToVec3() + front, up);
        }
    }

    public void OnMouseMove(IMouse mouse, Vector2 position)
    {
        float xoffset = position.X - lastX;
        float yoffset = lastY - position.Y; // reversed since y-coordinates range from bottom to top
        lastX = position.X;
        lastY = position.Y;

        const float sensitivity = 0.1f;
        xoffset *= sensitivity;
        yoffset *= sensitivity;

        yaw += xoffset;
        pitch += yoffset;

        if (pitch > 89.0f)
            pitch = 89.0f;
        if (pitch < -89.0f)
            pitch = -89.0f;

        vec3 direction;
        direction.x = MathF.Cos(glm.Radians(yaw)) * MathF.Cos(glm.Radians(pitch));
        direction.y = MathF.Sin(glm.Radians(pitch));
        direction.z = MathF.Sin(glm.Radians(yaw)) * MathF.Cos(glm.Radians(pitch));
        front = glm.Normalized(direction);
    }
}