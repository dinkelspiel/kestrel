namespace Kestrel.Framework.Buffers;

public interface IGLBuffer
{
    uint Buffer { set; get; }
    void Bind();
}