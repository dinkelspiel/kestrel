using Kestrel.Framework.Server;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Networking.Packets;

public interface IPacket<T>
{
    ushort PacketId { get; }
    void Serialize(NetDataWriter writer);
    void Deserialize(NetDataReader reader);

    void Handle(T context, NetPeer peer);
}

public interface IC2SPacket : IPacket<ServerState> { }

public interface IS2CPacket : IPacket<ClientState> { }