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
        byte[] ConnectionKey { get; set; }
        void EnableAes();
        void SendPacket(Packet packet);
        Task SendPacketAsync(Packet packet);
        Task<Packet> ReceivePacketAsync();
        event EventHandler<PacketReceivedEventArgs> PacketReceived;
        bool IsConnected { get; }
        event EventHandler ConnectionLost;
        int ProtocolVersion { get; set; }
        void StartListening();
        Task StopListeningAsync();
        void AddCustomPacket<type>(byte id) where type : Packet, new();
    }
}
