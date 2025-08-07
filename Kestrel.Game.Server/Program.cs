namespace Kestrel.Framework.Server;

using Kestrel.Framework;

public class Program
{
    public static void Main(string[] args)
    {
        Server server = new();
        server.Run();
    }
}