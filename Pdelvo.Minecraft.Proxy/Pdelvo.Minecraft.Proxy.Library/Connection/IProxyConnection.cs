using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Protocol.Packets;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    /// <summary>
    /// Defines a interface a proxy connection must implement
    /// </summary>
    public interface IProxyConnection : IDisposable
    {
        /// <summary>
        /// Gets the server end point of this connection
        /// </summary>
        IProxyEndPoint ServerEndPoint { get; }

        /// <summary>
        /// Gets the client end point of this connection
        /// </summary>
        IProxyEndPoint ClientEndPoint { get; }

        /// <summary>
        /// Starts listening for server packets
        /// </summary>
        void StartServerListening();

        /// <summary>
        /// Starts listening for client packets
        /// </summary>
        void StartClientListening();

        /// <summary>
        /// Get the username of the user which is currently connected through this connection
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the host the user used to connect to the proxy server
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Start closing the connection
        /// </summary>
        /// <returns>A task which can be used to track the closing</returns>
        Task CloseAsync();

        /// <summary>
        /// Connect to a new server and return asynchrony the LogOnResponse packet, or a DisconnectPacket
        /// </summary>
        /// <param name="serverEndPoint">Information about the new backend server</param>
        /// <returns>The resulting packet of this connection process</returns>
        Task<Packet> InitializeServerAsync(RemoteServerInfo serverEndPoint);

        /// <summary>
        /// Gets the entity of the user
        /// </summary>
        int EntityID { get; }

        /// <summary>
        /// Asyncronously kicking a client
        /// </summary>
        /// <param name="message">The kick message</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task KickUserAsync(string message);
    }
}