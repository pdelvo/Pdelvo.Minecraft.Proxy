using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Protocol;
using Pdelvo.Minecraft.Protocol.Packets;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    /// <summary>
    /// This is a base implementation of the IProxyEndPoint interface used in the default implementation of this proxy server
    /// </summary>
    public class ProxyEndPoint : IProxyEndPoint
    {
        /// <summary>
        /// Get or set the remote interface this end point belongs to
        /// </summary>
        protected RemoteInterface RemoteInterface { get; set; }

        /// <summary>
        /// Create a new instance using the RemoteInterface
        /// </summary>
        /// <param name="inter">The remote interface</param>
        public ProxyEndPoint(RemoteInterface inter)
        {
            RemoteInterface = inter;
        }

        /// <summary>
        /// Create a new instance using the RemoteInterface and the protocol version. 
        /// The protocol version overrides the version set in the remote interface.
        /// </summary>
        /// <param name="inter">The remote interface</param>
        /// <param name="protocolVersion">The protocol version</param>
        public ProxyEndPoint(RemoteInterface inter, int protocolVersion)
        {
            RemoteInterface = inter;
            ProtocolVersion = protocolVersion;
        }

        /// <summary>
        /// Get or sets the protocol version which should be used
        /// </summary>
        public int ProtocolVersion
        {
            get
            {
                return RemoteInterface.EndPoint.Version;
            }
            set
            {
                RemoteInterface.EndPoint.Version = value;
            }
        }

        /// <summary>
        /// Add a packet to the sending queue
        /// </summary>
        /// <param name="packet">The packet which should be sent</param>
        public void SendPacket(Packet packet)
        {
            RemoteInterface.SendPacketQueued(packet);
        }

        /// <summary>
        /// Asynchronously send a packet the the other end point and return a task to observe the sending process
        /// </summary>
        /// <param name="packet">The packet which should be sent</param>
        /// <returns>A task which can be used to observe the sending process</returns>
        public Task SendPacketAsync(Packet packet)
        {
            return RemoteInterface.SendPacketAsync(packet);
        }

        /// <summary>
        /// Get called when a packet is received after calling StartListening
        /// </summary>
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        /// <summary>
        /// True if the end point is currently connected to a remote end point, otherwise false
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Get called when the connection is terminated
        /// </summary>
        public event EventHandler ConnectionLost;

        /// <summary>
        /// Start listening for packets
        /// </summary>
        public void StartListening()
        {
            RemoteInterface.PacketReceived += OnPacketReceived;
            RemoteInterface.Aborted += RemoteInterface_Aborted;

            var r = RemoteInterface.Run();
        }

        void RemoteInterface_Aborted(object sender, RemoteInterfaceAbortedEventArgs e)
        {
            if (ConnectionLost != null)
                ConnectionLost(this, EventArgs.Empty);
        }

        /// <summary>
        /// Calls the packet received event
        /// </summary>
        /// <param name="sender">The sending object of the packet</param>
        /// <param name="e">Parameters which contains information about the sendt packet</param>
        protected virtual void OnPacketReceived(object sender, PacketEventArgs e)
        {
            if (PacketReceived != null)
                PacketReceived(this, new PacketReceivedEventArgs(e.Packet));
        }

        /// <summary>
        /// Add a packet to the sending queue
        /// </summary>
        /// <param name="packet">The packet which should be sent</param>
        public void SendPacketQueued(Packet packet)
        {
            RemoteInterface.SendPacketQueued(packet);
        }

        /// <summary>
        /// Stop listen for new packets asynchronously
        /// </summary>
        /// <returns></returns>
        public virtual Task StopListeningAsync()
        {
            RemoteInterface.Shutdown();
            return Task.FromResult(0);
        }

        /// <summary>
        /// Add a packet to the underlying remote interface, so it can be parsed
        /// </summary>
        /// <typeparam name="type">The type of the packet object</typeparam>
        /// <param name="id">The id of the packet</param>
        public void AddCustomPacket<type>(byte id) where type : Packet, new()
        {
            RemoteInterface.RegisterPacket<type>(id);
        }

        /// <summary>
        /// Asynchronously receive a packet
        /// </summary>
        /// <returns>A packet which got sent from the remote end point</returns>
        public Task<Packet> ReceivePacketAsync()
        {
            return RemoteInterface.ReadPacketAsync();
        }

        /// <summary>
        /// The shared Aes connection key
        /// </summary>
        public byte[] ConnectionKey { get; set; }

        /// <summary>
        /// Switch to Aes mode
        /// </summary>
        public void EnableAes()
        {
            RemoteInterface.SwitchToAesMode(ConnectionKey);
        }

        /// <summary>
        /// Close the underlying connection
        /// </summary>
        public void Close()
        {
            RemoteInterface.Shutdown();
        }

        /// <summary>
        /// Get the remote end point of this connection
        /// </summary>
        public System.Net.IPEndPoint RemoteEndPoint { get; internal set; }
    }
}
