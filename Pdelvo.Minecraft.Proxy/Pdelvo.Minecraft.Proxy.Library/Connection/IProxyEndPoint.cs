using System;
using System.Net;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Protocol.Packets;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    /// <summary>
    ///   A interface a proxy end point must implement
    /// </summary>
    public interface IProxyEndPoint
    {
        /// <summary>
        ///   Gets or Sets the shared key of this end point
        /// </summary>
        byte[] ConnectionKey { get; set; }

        /// <summary>
        ///   True if the endpoint is currently connected to a remote end point
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///   Get or Set the current protocol version of this end point
        /// </summary>
        int ProtocolVersion { get; set; }

        /// <summary>
        ///   The remote IP end point
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        ///   switch to aes encryption with the current <see cref="ConnectionKey">ConnectionKey</see>
        /// </summary>
        void EnableAes();

        /// <summary>
        ///   Synchronously send a packet
        /// </summary>
        /// <param name="packet"> The packet which should be sent </param>
        void SendPacket(Packet packet);

        /// <summary>
        ///   Asynchronously send a packet
        /// </summary>
        /// <param name="packet"> The packet which should be sent </param>
        /// <returns> A Task to observe the sending process </returns>
        Task SendPacketAsync(Packet packet);

        /// <summary>
        ///   Asynchronously waiting for a packet
        /// </summary>
        /// <returns> A task representating the incoming packet </returns>
        Task<Packet> ReceivePacketAsync();

        /// <summary>
        ///   A event which get called every time a packet is received after calling <see cref="StartListening">StartListening</see>
        /// </summary>
        event EventHandler<PacketReceivedEventArgs> PacketReceived;

        /// <summary>
        ///   A event which get called when the connection to the remote end point is terminated
        /// </summary>
        event EventHandler ConnectionLost;

        /// <summary>
        ///   Start waiting for new packets
        /// </summary>
        void StartListening();

        /// <summary>
        ///   Asynchronously stop waiting for new packets
        /// </summary>
        /// <returns> </returns>
        Task StopListeningAsync();

        /// <summary>
        ///   Add a custom packet so it can be parsed by the server
        /// </summary>
        /// <typeparam name="type"> The packet which should get added to the system </typeparam>
        /// <param name="id"> The id of the packet </param>
        void AddCustomPacket<type>(byte id) where type : Packet, new ();

        /// <summary>
        ///   Add a packet to the sending queue
        /// </summary>
        /// <param name="packet"> The packet which should be sent </param>
        void SendPacketQueued(Packet packet);

        /// <summary>
        ///   Close the end point
        /// </summary>
        void Close();
    }
}