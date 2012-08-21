using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    /// <summary>
    /// This interface must be implemented by each class which should be used as a packet listener in the proxy server
    /// </summary>
    public interface IPacketListener
    {
        /// <summary>
        /// This method will be called when a packet comes from the client and should be sent to the server
        /// </summary>
        /// <param name="e">information about the packet which should be sent</param>
        void ClientPacketReceived(PacketReceivedEventArgs e);

        /// <summary>
        /// This method will be called when a packet comes from the server and should be sent to the client
        /// </summary>
        /// <param name="e">information about the packet which should be sent</param>
        void ServerPacketReceived(PacketReceivedEventArgs e);
    }
}
