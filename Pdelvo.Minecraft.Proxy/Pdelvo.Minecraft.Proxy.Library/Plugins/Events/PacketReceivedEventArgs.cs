using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Protocol.Packets;
using Pdelvo.Minecraft.Proxy.Library.Connection;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    public class PacketReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// If this is set to true the packet will not be sent to the other end point
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// The Connection this event belongs to
        /// </summary>
        public IProxyConnection Connection { get; set; }

        /// <summary>
        /// The packet which should be redirected
        /// </summary>
        public Packet Packet { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="PacketReceivedEventArgs"/> class
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="connection"></param>
        public PacketReceivedEventArgs(Packet packet, IProxyConnection connection)
        {
            Packet = packet;
            Connection = connection;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PacketReceivedEventArgs"/> class
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="connection"></param>
        public PacketReceivedEventArgs(Packet packet)
        {
            Packet = packet;
        }
    }
}
