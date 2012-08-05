using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Protocol;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    public class ProxyConnection : IProxyConnection
    {
        Socket _networkSocket;
        ProxyServer _server;
        ProxyEndPoint _serverEndPoint;
        ProxyEndPoint _clientEndPoint;

        public ProxyConnection(Socket networkSocket, ProxyServer server)
        {
            _networkSocket = networkSocket;
            _server = server;
        }

        public Task CloseAsync()
        {
            return Task.FromResult(0);
        }

        public void Dispose()
        {

        }

        internal virtual async void HandleClient()
        {
        }

        public IProxyEndPoint ServerEndPoint
        {
            get { return _serverEndPoint; }
        }

        public IProxyEndPoint ClientEndPoint
        {
            get { return _clientEndPoint; }
        }
    }
}
