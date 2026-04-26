using System.Numerics;
using Silk.NET.Input;
using Arch.Core.Extensions;
using Kestrel.Client.ECS;

namespace Kestrel.Client.Renderer;

public class Camera(ClientContext clientContext)
{
    public float Yaw = -90f;
    public float Pitch = 20f;
    public float LastMouseX, LastMouseY;
    public bool FirstMouse = true;
    public float Sensitivity = 0.1f;
    public float Speed = 6f;
    public float CameraDistance = 4.5f;
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

    public Matrix4x4 GetViewRotation()
    {
        float yawRad = MathF.PI / 180f * clientContext.camera.Yaw;
        float pitchRad = MathF.PI / 180f * clientContext.camera.Pitch;
        return Matrix4x4.Transpose(Matrix4x4.Identity * Matrix4x4.CreateRotationY(yawRad + (float)Math.PI / 2) * Matrix4x4.CreateRotationX(-pitchRad));
    }

    public Matrix4x4 GetViewMatrix()
    {
        var cameraTarget = Vector3.Zero;
        if (clientContext.TryGetPlayer(out var player))
            cameraTarget = player.Get<TransformComponent>().Postition;

        float yawRad = MathF.PI / 180f * clientContext.camera.Yaw;
        float pitchRad = MathF.PI / 180f * clientContext.camera.Pitch;
        var cameraOffset = new Vector3(
            clientContext.camera.CameraDistance * MathF.Cos(pitchRad) * MathF.Cos(yawRad),
            clientContext.camera.CameraDistance * MathF.Sin(pitchRad),
            clientContext.camera.CameraDistance * MathF.Cos(pitchRad) * MathF.Sin(yawRad)
        );
        var cameraRight = Vector3.Normalize(Vector3.Cross(-cameraOffset, Vector3.UnitY));
        var shoulder = cameraRight * clientContext.camera.CameraShoulderOffset;
        var head = Vector3.UnitY * clientContext.camera.CameraHeightOffset;
        var cameraPos = cameraTarget + cameraOffset + shoulder + head;
        var view = Matrix4x4.CreateLookAt(cameraPos, cameraTarget + shoulder + head, Vector3.UnitY);
        return view;
    }
}