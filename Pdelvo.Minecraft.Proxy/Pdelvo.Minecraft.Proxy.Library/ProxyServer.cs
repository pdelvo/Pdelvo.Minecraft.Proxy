using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pdelvo.Async.Extensions;
using Pdelvo.Minecraft.Proxy.Library.Plugins;

namespace Pdelvo.Minecraft.Proxy.Library
{
    public class ProxyServer : IProxyServer
    {
        IPEndPoint _localEndPoint;
        volatile int _connectedUsers;
        bool _listening;
        bool _acceptingNewClients;
        SynchronizedCollection<ProxyConnection> _openConnection;
        Socket _listeningSocket;
        PluginManager _pluginManager;

        public ProxyServer(IPEndPoint endPoint)
        {
            _localEndPoint = endPoint;
            _pluginManager = new PluginManager();
            _pluginManager.LoadPlugins();
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

            ReceiveClientsAsync();
        }

        private async void ReceiveClientsAsync()
        {
            while (true)
            {
                try
                {
                    var remote = await _listeningSocket.AcceptTaskAsync();

                    IPAddress address = ((IPEndPoint)remote.RemoteEndPoint).Address;

                    if (!(PluginManager.TriggerPlugin.AllowJoining(address) ?? false))
                    {
                        remote.Close();
                        Console.WriteLine("Denied access from " + address);
                        continue;
                    }
                    //Connection accepted

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

        public IEnumerable<IProxyConnection> OpenConnections
        {
            get
            {
                return _openConnection;
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

        public PluginManager PluginManager
        {
            get { return _pluginManager; }
        }
    }
}
