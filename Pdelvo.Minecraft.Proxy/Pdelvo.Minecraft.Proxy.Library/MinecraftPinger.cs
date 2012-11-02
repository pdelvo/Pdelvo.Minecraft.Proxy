using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Pdelvo.Async.Extensions;
using Pdelvo.Minecraft.Protocol;
using Pdelvo.Minecraft.Protocol.Packets;

namespace Pdelvo.Minecraft.Proxy.Library
{
    /// <summary>
    /// Provides methods to ping a minecraft server
    /// </summary>
    public static class MinecraftPinger
    {
        /// <summary>
        /// Pings a minecraft server and returns the result string
        /// </summary>
        /// <param name="remoteAddress">The server adress</param>
        /// <returns>The resulting string</returns>
        public static async Task<string> PingServerAsync(IPEndPoint remoteAddress)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            await socket.ConnectTaskAsync(remoteAddress);

            using (var networkStream = new NetworkStream(socket))
            {
                var remoteInterface = ServerRemoteInterface.Create(networkStream, 48);

                await remoteInterface.SendPacketAsync(new PlayerListPing { MagicByte = 1 });

                var result = await remoteInterface.ReadPacketAsync();

                var disconnectPacket = result as DisconnectPacket;
                if (disconnectPacket != null)
                    return disconnectPacket.Reason;
                throw new PacketException("Server return invalid packet");
            }
        }
        /// <summary>
        /// Pings a minecraft server and returns the result string
        /// </summary>
        /// <param name="remoteAddress">The server adress</param>
        /// <returns>The resulting string</returns>
        public static async Task<ServerPingInformation> GetServerInformationAsync(IPEndPoint remoteAddress)
        {
            var result = await PingServerAsync(remoteAddress);

            var parts = result.Split(new char[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 1)
            {
                var information = new ServerPingInformation
                                      {
                                          ProtocolVersion = int.Parse(parts[1]),
                                          VersionString = parts[2],
                                          MotD = parts[3],
                                          UsedSlots = int.Parse(parts[4]),
                                          TotalSlots = int.Parse(parts[5])
                                      };

                return information;
            }
            else
            {
                parts = result.Split(new char[] { '§' }, StringSplitOptions.RemoveEmptyEntries);
                var information = new ServerPingInformation
                {
                    MotD = parts[0],
                    UsedSlots = int.Parse(parts[1]),
                    TotalSlots = int.Parse(parts[2])
                };
                return information;
            }
        }
    }

    /// <summary>
    /// This class provides information of a server ping result
    /// </summary>
    public class ServerPingInformation
    {
        /// <summary>
        /// The protocol version of the remote server, null if the information is not present
        /// </summary>
        public int? ProtocolVersion { get; set; }

        /// <summary>
        /// A string representing the version of the server, null if the information is not present
        /// </summary>
        public string VersionString { get; set; }

        /// <summary>
        /// The message of the day of the remote server
        /// </summary>
        public string MotD { get; set; }

        /// <summary>
        /// The currently used slots
        /// </summary>
        public int UsedSlots { get; set; }

        /// <summary>
        /// The total slots
        /// </summary>
        public int TotalSlots { get; set; }
    }
}