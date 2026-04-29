using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Kestrel.Client.ECS;
using Kestrel.Client.Mesh;
using Kestrel.Client.Renderer;
using Kestrel.Client.Renderer;
using Kestrel.Client.Scene;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Shader = Kestrel.Client.Renderer.Shader;
using Texture = Kestrel.Client.Renderer.Texture;

namespace Kestrel.Client.Scenes;

public class GameScene(ClientContext clientContext) : SceneBase(clientContext)
{
    const float Gravity = 25f;
    const float JumpSpeed = 12f;
    public Camera camera;
    public RenderPass renderPass;

    public override unsafe void Load()
    {
        var gl = clientContext.Gl;

        clientContext.Mouse.Cursor.CursorMode = CursorMode.Raw;
        clientContext.Mouse.MouseMove += OnMouseMove;

        camera = new(clientContext);
        clientContext.camera = camera;

        renderPass = new(clientContext);
        renderPass.Setup();
    }

    public override void Update(double dt)
    {
        var input = clientContext.Input;
        float step = (float)dt;

        float yawRad = MathF.PI / 180f * camera.Yaw; var forward = Vector3.Normalize(new Vector3(MathF.Cos(yawRad), 0f, MathF.Sin(yawRad)));
        var right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));

        if (clientContext.TryGetPlayer(out var player))
        {
            var wish = Vector3.Zero;
            if (input.IsKeyDown(Key.W)) wish -= forward;
            if (input.IsKeyDown(Key.S)) wish += forward;
            if (input.IsKeyDown(Key.A)) wish += right;
            if (input.IsKeyDown(Key.D)) wish -= right;
            if (wish.LengthSquared() > 0f) wish = Vector3.Normalize(wish);

            ref var vel = ref player.Get<VelocityComponent>();
            vel.Velocity.X = wish.X * camera.Speed;
            vel.Velocity.Z = wish.Z * camera.Speed;
            if (input.IsKeyPressed(Key.Space) && player.Get<TransformComponent>().IsGrounded)
                vel.Velocity.Y = JumpSpeed;
            // if (input.IsKeyPressed(Key.Space))
            // if (input.IsKeyPressed(Key.ShiftLeft))
            // vel.Velocity.Y = 0;
            player.Get<TransformComponent>().Yaw = camera.Yaw;
        }

        clientContext.World.Query(new QueryDescription().WithAll<VelocityComponent, TransformComponent>(), (ref TransformComponent transform, ref VelocityComponent velocity) =>
        {
            velocity.Velocity.Y -= Gravity * step;
        });

        clientContext.World.Query(new QueryDescription().WithAll<TransformComponent, VelocityComponent, HeightmapColliderComponent>(), (ref TransformComponent transform, ref VelocityComponent velocity) =>
        {
            float[,] heightmap = HeightmapDrawInstruction.Heightmap;
            int size = HeightmapDrawInstruction.Size;

            Vector3 minCorner3D = transform.Postition - new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 maxCorner3D = transform.Postition + new Vector3(0.5f, -0.5f, 0.5f);
            Vector2 extremeMinCorner = new(MathF.Floor(minCorner3D.X), MathF.Floor(minCorner3D.Z));
            Vector2 extremeMaxCorner = new(MathF.Ceiling(maxCorner3D.X), MathF.Ceiling(maxCorner3D.Z));

            float minHeight = heightmap[(int)extremeMinCorner.X, (int)extremeMinCorner.Y];
            float maxHeight = heightmap[(int)extremeMaxCorner.X, (int)extremeMaxCorner.Y];

            float lerpedHeight = minHeight + 0.5f * (maxHeight - minHeight);

            if (transform.Postition.Y + velocity.Velocity.Y * step < lerpedHeight + 0.5f)
            {
                transform.Postition.Y = lerpedHeight + 0.5f;
                transform.IsGrounded = true;
                velocity.Velocity.Y = 0;
            }

            // renderPass.DrawCube(Matrix4x4.CreateTranslation(new(extremeMinCorner.X, heightmap[(int)extremeMinCorner.X, (int)extremeMinCorner.Y], extremeMinCorner.Y)), (0, 0));
            // renderPass.DrawCube(Matrix4x4.CreateTranslation(new(extremeMaxCorner.X, heightmap[(int)extremeMaxCorner.X, (int)extremeMaxCorner.Y], extremeMaxCorner.Y)), (0, 0));
        });

        clientContext.World.Query(new QueryDescription().WithAll<VelocityComponent, TransformComponent>(), (ref TransformComponent transform, ref VelocityComponent velocity) =>
        {
            transform.Postition += velocity.Velocity * step;

            // if (transform.Postition.Y < 0f)
            // {
            //     transform.Postition.Y = 0f;
            //     if (velocity.Velocity.Y < 0f) velocity.Velocity.Y = 0f;
            // }
        });
    }

    public override void Render(double dt)
    {
        var gl = clientContext.Gl;
        renderPass.Begin();
        renderPass.DrawCube(Matrix4x4.Identity * Matrix4x4.CreateTranslation(0, -1, 0), (0, 0));
        renderPass.DrawBillboard(Matrix4x4.Identity * Matrix4x4.CreateTranslation(0, 0, 0), (2, 0));

        clientContext.World.Query(new QueryDescription().WithAll<PlayerTag, TransformComponent>(), (ref TransformComponent transform) =>
        {
            float pYawRad = MathF.PI / 180f * transform.Yaw;
            var model = Matrix4x4.CreateRotationY(MathF.PI - pYawRad) * Matrix4x4.CreateTranslation(transform.Postition);
            renderPass.DrawCube(model, (1, 0));
        });

        renderPass.DrawHeightmap(Matrix4x4.Identity, (0, 0));

        renderPass.End();
    }

    public override void Unload()
    {
        clientContext.Mouse.MouseMove -= OnMouseMove;
        renderPass.CleanUp();
    }

    void OnMouseMove(IMouse mouse, Vector2 position)
    {
        camera.OnMouseMove(mouse, position);
    }
}
