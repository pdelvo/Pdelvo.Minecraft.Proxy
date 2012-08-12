using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Protocol.Packets;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    public interface IProxyConnection : IDisposable
    {
        IProxyEndPoint ServerEndPoint { get; }
        IProxyEndPoint ClientEndPoint { get; }

        void StartServerListening();
        void StartClientListening();

        string Username { get; }
        string Host { get; }

        Task CloseAsync();
        Task<Packet> InitializeServerAsync(RemoteServerInfo serverEndPoint);
    }
}
