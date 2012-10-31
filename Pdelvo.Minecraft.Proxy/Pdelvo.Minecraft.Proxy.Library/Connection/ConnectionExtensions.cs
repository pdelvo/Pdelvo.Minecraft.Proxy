using Pdelvo.Minecraft.Protocol.Packets;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    /// <summary>
    ///   Provides extension methods to use the IProxyExtensions simpler
    /// </summary>
    public static class ConnectionExtensions
    {
        /// <summary>
        ///   Send a packet to the server end point of the IProxyConnection
        /// </summary>
        /// <param name="connection"> The connection </param>
        /// <param name="packet"> The packet which should be sent </param>
        public static void SendServerPacket(this IProxyConnection connection, Packet packet)
        {
            connection.ServerEndPoint.SendPacket(packet);
        }

        /// <summary>
        ///   Send a packet to the client end point of the IProxyConnection
        /// </summary>
        /// <param name="connection"> The connection </param>
        /// <param name="packet"> The packet which should be sent </param>
        public static void SendClientPacket(this IProxyConnection connection, Packet packet)
        {
            connection.ClientEndPoint.SendPacket(packet);
        }
    }
}