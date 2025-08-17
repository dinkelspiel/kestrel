using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Utils;
using Kestrel.Framework.World;

namespace Kestrel.Framework.Server.Player;

public class ClientPlayer
{
    public string Name { get; set; }
    public Vector3 Location { get; set; }
    public Vector3I LastFrameChunkPos { get; set; }
}