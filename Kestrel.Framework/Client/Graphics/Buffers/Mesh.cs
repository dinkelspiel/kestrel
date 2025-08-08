namespace Kestrel.Framework.Client.Graphics.Buffers;

public abstract class Mesh(ClientState clientState)
{
    public float[] Vertices = [];
    public uint vbo, vao;

    public abstract unsafe void Generate();

    public void Bind() => clientState.Window.GL.BindVertexArray(vao);
}