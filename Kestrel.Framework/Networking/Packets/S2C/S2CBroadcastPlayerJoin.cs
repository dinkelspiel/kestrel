// using System.Numerics;
// using GlmSharp;
// // using LiteNetLib;
// using LiteNetLib.Utils;

// namespace Kestrel.Framework.Networking.Packets.S2C;

// public class S2CBroadcastPlayerJoin : IPacket
// {
//     public byte PacketId => 5;
//     public string PlayerName;
//     public Vector3 Position;

//     public void Deserialize(NetDataReader reader)
//     {
//         PlayerName = reader.GetString(64);
//         Position = new()
//         {
//             X = reader.GetFloat(),
//             Y = reader.GetFloat(),
//             Z = reader.GetFloat()
//         };
//     }

//     public void Serialize(NetDataWriter writer)
//     {
//         writer.Put(PlayerName, 64);
//         writer.Put(Position.X);
//         writer.Put(Position.Y);
//         writer.Put(Position.Z);
//     }

//     public void Handle(ClientState context, NetPeer server)
//     {
//         if (PlayerName == context.Player.Name)
//         {
//             return;
//         }

//         if (!context.Players.ContainsKey(PlayerName))
//         {
//             context.Players.TryAdd(PlayerName, new ClientPlayer()
//             {
//                 Name = PlayerName,
//                 Location = new Vector3(Position.X, Position.Y, Position.Z)
//             });
//         }
//         else
//         {
//             ClientPlayer player = context.Players[PlayerName];
//             player.Location = new Vector3(Position.X, Position.Y, Position.Z);
//         }
//     }
// }