using System.Collections.Concurrent;
using Arch.Core;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.Networking;
using Kestrel.Framework.Networking.Packets;
using Kestrel.Framework.Networking.Packets.C2S;
using Kestrel.Framework.Utils;
using LiteNetLib;
using LiteNetLib.Utils;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;

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
        ServerState.Entities = ArchWorld.Create();

        ServerState.Entities.Create(new Location(ServerState.World, -416, 80, 383), new ModelRenderer(""));

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

            ServerState.Entities.Query(new QueryDescription().WithAll<Location>(), (ArchEntity entity, ref Location location) =>
            {
                if (LocationUtil.Distance(new(location.X, location.Y, location.Z), new(location.LastUpdatedX, location.LastUpdatedY, location.LastUpdatedZ)) > 5)
                {

                }
            });
        }
        ServerState.NetServer.Stop();
    }
}
