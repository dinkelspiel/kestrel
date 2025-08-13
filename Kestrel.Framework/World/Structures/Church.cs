namespace Kestrel.Framework.World.Structures;

public class StructureChurch : Structure
{
    public override int GetWidth() => 5;

    public override BlockType[,] GetBlocks()
    {
        return new BlockType[,]
        {
            { BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone,
              BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone,
              BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Dirt,
              BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone,
              BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone } ,
            { BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone,
              BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Stone,
              BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Dirt,
              BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Stone,
              BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone } ,
            { BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone,
              BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Dirt,
              BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Dirt,
              BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Dirt,
              BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone, BlockType.Stone },
            { BlockType.Stone, BlockType.Air, BlockType.Stone, BlockType.Stone, BlockType.Stone,
              BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Stone,
              BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Dirt,
              BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Stone,
              BlockType.Stone, BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Stone },
            { BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air,
              BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Stone,
              BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Stone,
              BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Stone,
              BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air },
            { BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air,
              BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air,
              BlockType.Stone, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Stone,
              BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air,
              BlockType.Air, BlockType.Air, BlockType.Air, BlockType.Air,BlockType.Air },
        };
    }
}