using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    public interface IPacketListener
    {
        void ClientPacketReceived(PacketReceivedEventArgs e);
        void ServerPacketReceived(PacketReceivedEventArgs e);
    }
}
