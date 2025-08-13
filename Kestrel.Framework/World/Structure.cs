namespace Kestrel.Framework.World;

public abstract class Structure
{
    public abstract BlockType[,] GetBlocks();
    public abstract int GetWidth();
}