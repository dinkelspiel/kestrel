namespace Kestrel.Game.Client;

using Kestrel.Framework;
using Kestrel.Framework.Platform;
using Silk.NET.Windowing;

public class Program
{
    public static void Main(string[] args)
    {
        Client client = new();
        client.Run();
    }
}