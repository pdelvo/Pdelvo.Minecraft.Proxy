using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library
{
    public class ProxyConnection : IProxyConnection
    {
        Socket _networkSocket;
        ProxyServer _server;

        public ProxyConnection(Socket networkSocket, ProxyServer server)
        {
            _networkSocket = networkSocket;
            _server = server;
        }

        public Task CloseAsync()
        {
            return null;
        }

        public void Dispose()
        {

        }
    }
}
