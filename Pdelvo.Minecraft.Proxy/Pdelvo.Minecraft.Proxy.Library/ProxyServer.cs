using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library
{
    public class ProxyServer : IProxyServer
    {
        IPEndPoint _localEndPoint;
        volatile int _connectedUsers;
        bool _listening;
        bool _acceptingNewClients;
        SynchronizedCollection<IProxyConnection> _openConnection;
        Socket _listeningSocket;
        CancellationTokenSource _shutdownTokenSource = new CancellationTokenSource();

        public ProxyServer(IPEndPoint endPoint)
        {
            _localEndPoint = endPoint;
        }

        public IPEndPoint LocalEndPoint
        {
            get { return _localEndPoint; }
        }

        public int ConnectedUsers
        {
            get { return _connectedUsers; }
        }

        public bool Listening
        {
            get { return _listening; }
        }

        public void Start()
        {
            if (_listening) throw new InvalidOperationException("Proxy server is running");

            _listeningSocket = new Socket(LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _listeningSocket.Bind(LocalEndPoint);

            _listeningSocket.Listen(10);

            ReceiveClientsAsync(_shutdownTokenSource.Token);
        }

        private async void ReceiveClientsAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    var remote = await _listeningSocket.AcceptTaskAsync();
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {

                }
            }
        }

        public async Task StopAsync()
        {
            if (!_listening) return;
            _acceptingNewClients = false;
            await Task.WhenAll(_openConnection.Select(a => a.CloseAsync()));

            _listeningSocket.Close();
            _listeningSocket.Dispose();
        }

        public virtual void Dispose()
        {

        }
    }
}
