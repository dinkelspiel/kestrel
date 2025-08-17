using System.Numerics;
using GlmSharp;
using LiteNetLib;

namespace Kestrel.Framework.Server.Player;

public class ServerPlayer
{
    public string Name { get; set; }
    public vec3 Location { get; set; }
    public NetPeer NetClient { get; set; }
}