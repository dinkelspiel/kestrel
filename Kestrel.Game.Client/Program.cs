namespace Kestrel.Game.Client;

using Kestrel.Framework;
using Kestrel.Framework.Platform;
using Silk.NET.Windowing;

public class Client : ClientBase
{

}

public class Desktop
{
    public static void Main(string[] args)
    {
        GameHost host = new();
        Client client = new();
        host.Run(client);
    }
}