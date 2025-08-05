using Silk.NET.OpenGL;

namespace Kestrel.Framework.Buffers;

public class ElementBuffer(GL gl, uint[] indices) : IGLBuffer
{
    public uint Buffer { get; set; } = gl.GenBuffer();

    public unsafe void Bind()
    {
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Buffer);

        fixed (uint* buf = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
    }
}