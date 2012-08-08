using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Protocol;
using Pdelvo.Minecraft.Protocol.Packets;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    public class ProxyEndPoint : IProxyEndPoint
    {
        protected RemoteInterface RemoteInterface;

        public ProxyEndPoint(RemoteInterface inter, int protocolVersion)
        {
            RemoteInterface = inter;
            ProtocolVersion = protocolVersion;
        }

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

        public void SendPacket(Packet packet)
        {
            RemoteInterface.SendPacketQueued(packet);
        }

        public Task SendPacketAsync(Packet packet)
        {
            return RemoteInterface.SendPacketAsync(packet);
        }

        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        public bool IsConnected { get; private set; }

        public event EventHandler ConnectionLost;


        public void StartListening()
        {
            RemoteInterface.PacketReceived += OnPacketReceived;
        }

        protected virtual void OnPacketReceived(object sender, PacketEventArgs e)
        {
            if (PacketReceived != null)
                PacketReceived(this, new PacketReceivedEventArgs(e.Packet));
        }

        public virtual Task StopListeningAsync()
        {
            RemoteInterface.Shutdown();
            return Task.FromResult(0);
        }

        public void AddCustomPacket<type>(byte id) where type : Packet, new()
        {
            RemoteInterface.RegisterPacket<type>(id);
        }
    }
}
