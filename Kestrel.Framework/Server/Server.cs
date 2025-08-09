using System.Collections.Concurrent;
using Kestrel.Framework.Networking;
using Kestrel.Framework.Networking.Packets;
using Kestrel.Framework.Networking.Packets.C2S;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Server;

public class Server
{
    public ServerState ServerState { get; private set; } = new();

    public void Run()
    {
        EventBasedNetListener listener = new();
        ServerState.NetServer = new(listener);
        ServerState.NetServer.Start(9050 /* port */);

        ServerState.World = new();

        PacketRegistry.RegisterPackets();

        listener.ConnectionRequestEvent += request =>
        {
            if (ServerState.NetServer.ConnectedPeersCount < 10 /* max connections */)
                request.AcceptIfKey("SomeConnectionKey");
            else
                request.Reject();
        };

        listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine("We got connection: {0}", peer);  // Show peer ip
                                                                // NetDataWriter writer = new();         // Create writer class
                                                                // writer.Put((ushort)1);
                                                                // writer.Put((ushort)2);
                                                                // writer.Put((ushort)3);


            // peer.Send(writer, DeliveryMethod.ReliableOrdered);  // Send with reliability
        };

        listener.NetworkReceiveEvent += (client, dataReader, deliveryMethod, channel) =>
        {
            IC2SPacket packet = PacketManager.DeserializeC2SPacket(dataReader);
            PacketManager.HandleC2SPacket(packet, ServerState, client);

            dataReader.Recycle();
        };


        while (!Console.KeyAvailable)
        {
            ServerState.NetServer.PollEvents();
            Thread.Sleep(15);
        }
        ServerState.NetServer.Stop();
    }
}
