using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Protocol.Packets;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    public interface IProxyEndPoint
    {
        void SendPacket(Packet packet);
        Task SendPacketAsync(Packet packet);
        event EventHandler<PacketReceivedEventArgs> PacketReceived;
        bool IsConnected { get; }
        event EventHandler ConnectionLost;
        int ProtocolVersion { get; }
        void StartListening();
        Task StopListeningAsync();
    }
}
