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
            RemoteInterface.Aborted += RemoteInterface_Aborted;

            var r = RemoteInterface.Run();
        }

        void RemoteInterface_Aborted(object sender, RemoteInterfaceAbortedEventArgs e)
        {
            if (ConnectionLost != null)
                ConnectionLost(this, EventArgs.Empty);
        }

        protected virtual void OnPacketReceived(object sender, PacketEventArgs e)
        {
            if (PacketReceived != null)
                PacketReceived(this, new PacketReceivedEventArgs(e.Packet));
        }

        public void SendPacketQueued(Packet packet)
        {
            RemoteInterface.SendPacketQueued(packet);
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


        public Task<Packet> ReceivePacketAsync()
        {
            return RemoteInterface.ReadPacketAsync();
        }

        public byte[] ConnectionKey { get; set; }

        public void EnableAes()
        {
            RemoteInterface.SwitchToAesMode(ConnectionKey);
        }


        public void Close()
        {
            RemoteInterface.Shutdown();
        }
    }
}
