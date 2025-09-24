namespace Kestrel.Framework.World;

public enum BlockType
{
    Air = 0,
    Stone = 1,
    Dirt = 2,
    Grass = 3,
    Water = 4,
    Leaves = 5
}

public static class BlockTypeExtensions
{
    public static bool IsSolid(this BlockType? blockType) => blockType != BlockType.Air;
    public static bool IsSolid(this BlockType blockType) => blockType != BlockType.Air;
}