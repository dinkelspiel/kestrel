using Kestrel.Framework.World;
using Silk.NET.OpenGL;

namespace Kestrel.Framework.Client.Graphics.Buffers;

public class CubeMesh(ClientState clientState) : Mesh(clientState)
{
    public override unsafe void Generate()
    {
        vao = clientState.Window.GL.GenVertexArray();
        clientState.Window.GL.BindVertexArray(vao);

        vbo = clientState.Window.GL.GenBuffer();
        clientState.Window.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        Mesher mesher = new();
        mesher.AddUpFace(0, 0, 0);
        mesher.AddDownFace(0, 0, 0);
        mesher.AddEastFace(0, 0, 0);
        mesher.AddWestFace(0, 0, 0);
        mesher.AddNorthFace(0, 0, 0);
        mesher.AddSouthFace(0, 0, 0);
        Vertices = mesher.Vertices.ToArray();

        fixed (float* buf = Vertices)
            clientState.Window.GL.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(Vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        clientState.Window.GL.EnableVertexAttribArray(0);
        clientState.Window.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);

        clientState.Window.GL.EnableVertexAttribArray(1);
        clientState.Window.GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));

        clientState.Window.GL.EnableVertexAttribArray(2);
        clientState.Window.GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(5 * sizeof(float)));

        // Clean up
        clientState.Window.GL.BindVertexArray(0);
        clientState.Window.GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }
}