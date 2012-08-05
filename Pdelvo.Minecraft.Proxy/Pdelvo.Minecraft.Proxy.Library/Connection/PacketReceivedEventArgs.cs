using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Protocol.Packets;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public Packet Packet { get; set; }
        public bool Handled { get; set; }

        public PacketReceivedEventArgs(Packet packet)
        {
            Packet = packet;
            Handled = false;
        }
    }
}
