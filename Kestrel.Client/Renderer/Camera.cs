using System.Numerics;
using Silk.NET.Input;

namespace Kestrel.Client.Renderer;

public class Camera
{
    public float Yaw = -90f;
    public float Pitch = 20f;
    public float LastMouseX, LastMouseY;
    public bool FirstMouse = true;
    public float Sensitivity = 0.1f;
    public float Speed = 6f;
    public float CameraDistance = 9f;
    public float CameraShoulderOffset = 1.5f;
    public float CameraHeightOffset = 1.5f;

    public void OnMouseMove(IMouse mouse, Vector2 position)
    {
        if (FirstMouse)
        {
            LastMouseX = position.X;
            LastMouseY = position.Y;
            FirstMouse = false;
            return;
        }

        float dx = (position.X - LastMouseX) * Sensitivity;
        float dy = (position.Y - LastMouseY) * Sensitivity;
        LastMouseX = position.X;
        LastMouseY = position.Y;

        Yaw += dx;
        Pitch = Math.Clamp(Pitch + dy, -89.9f, 89.9f);
    }
}