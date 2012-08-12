using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using log4net;
using Pdelvo.Async.Extensions;
using Pdelvo.Minecraft.Network;
using Pdelvo.Minecraft.Proxy.Library.Configuration;
using Pdelvo.Minecraft.Proxy.Library.Connection;
using Pdelvo.Minecraft.Proxy.Library.Plugins;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;

namespace Pdelvo.Minecraft.Proxy.Library
{
    public class ProxyServer : IProxyServer
    {
        bool _listening;
        bool _acceptingNewClients;
        SynchronizedCollection<ProxyConnection> _openConnection;
        SynchronizedCollection<ProxyConnection> _connectedUsers;
        Socket _listeningSocket;
        PluginManager _pluginManager;
        ILog _logger;
        internal RSAParameters RSAKeyPair { get; private set; }
        internal RSACryptoServiceProvider RSACryptoServiceProvider { get; private set; }
        public bool AcceptingNewClients { get { return _acceptingNewClients; } }

        public ProxyServer()
        {
            _logger = LogManager.GetLogger("Proxy Server");
            _pluginManager = new PluginManager(this);
            _pluginManager.LoadPlugins();
            _openConnection = new SynchronizedCollection<ProxyConnection>();
            _connectedUsers = new SynchronizedCollection<ProxyConnection>();
            ReadConfig();
        }

        public IPEndPoint LocalEndPoint { get; private set; }

        public int ConnectedUsers
        {
            get { return _connectedUsers.Count; }
        }

        public int MaxUsers { get; set; }
        public bool OnlineMode { get; set; }
        public string MotD { get; set; }

        public bool Listening
        {
            get { return _listening; }
        }

        public void Start()
        {
            if (_listening) throw new InvalidOperationException("Proxy server is running");

            _listening = true;

            RSACryptoServiceProvider rsa;

            RSAKeyPair = ProtocolSecurity.GenerateRSAKeyPair(out rsa);

            RSACryptoServiceProvider = rsa;

            _logger.Info("RSA Key pair generated");


            _listeningSocket = new Socket(LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _listeningSocket.Bind(LocalEndPoint);

            _logger.InfoFormat("Server is listening at {0}.", LocalEndPoint);

            _listeningSocket.Listen(10);

            ReceiveClientsAsync();
        }

        void ReadConfig()
        {
            var settings = ProxyConfigurationSection.Settings;
            MotD = settings.Motd;
            MaxUsers = settings.MaxPlayers;
            LocalEndPoint = Extensions.ParseEndPoint(settings.LocalEndPoint);
            OnlineMode = settings.OnlineMode;
        }

        private async void ReceiveClientsAsync()
        {
            _acceptingNewClients = true;
            while (true)
            {
                try
                {
                    var remote = await _listeningSocket.AcceptTaskAsync();

                    IPAddress address = ((IPEndPoint)remote.RemoteEndPoint).Address;

                    if (!(PluginManager.TriggerPlugin.AllowJoining(address) ?? false))
                    {
                        remote.Close();
                        _logger.Warn("Denied access from " + address);
                        continue;
                    }
                    //Connection accepted

                    ProxyConnection proxyConnection = new ProxyConnection(remote, this);

                    _openConnection.Add(proxyConnection);


                    proxyConnection.HandleClient();
                }
                catch (TaskCanceledException)
                {
                    _acceptingNewClients = false;
                    return;
                }
                catch (Exception)
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
            _listening = false;
            _acceptingNewClients = false;
            await Task.WhenAll(_openConnection.Select(a => a.CloseAsync()));

            _listeningSocket.Close();
            _listeningSocket.Dispose();
        }

        public virtual void Dispose()
        {
            StopAsync().Wait();
        }

        public PluginManager PluginManager
        {
            get { return _pluginManager; }
        }

        internal void RemoveConnection(ProxyConnection proxyConnection)
        {
            _openConnection.Remove(proxyConnection);
            if (_connectedUsers.Contains(proxyConnection))
            {
                PluginManager.TriggerPlugin.OnConnectionLost(new UserEventArgs(proxyConnection));
                _connectedUsers.Remove(proxyConnection);
            }
        }

        internal void PromoteConnection(ProxyConnection proxyConnection)
        {
            _connectedUsers.Add(proxyConnection);
        }


        public RemoteServerInfo GetServerEndPoint(IProxyConnection proxyConnection)
        {

            var settings = ProxyConfigurationSection.Settings;

            var server = settings.Server.OfType<ServerElement>().Where(m => m.IsDefault || (m.DnsName != null && proxyConnection.Host.StartsWith(m.DnsName))).OrderBy(m => m.IsDefault);

            var possibleResult = server.FirstOrDefault();
            var result = possibleResult == null ? null :new RemoteServerInfo(possibleResult.Name, Extensions.ParseEndPoint(possibleResult.EndPoint), possibleResult.MinecraftVersion);

            PluginResultEventArgs<RemoteServerInfo> args = new PluginResultEventArgs<RemoteServerInfo>(result, proxyConnection);
            PluginManager.TriggerPlugin.OnPlayerServerSelection(args);
            args.EnsureSuccess();
            return args.Result;
        }

        public bool OnlineModeEnabled(ProxyConnection proxyConnection)
        {
            return OnlineMode; //TODO: Add plugin support
        }
    }
}
