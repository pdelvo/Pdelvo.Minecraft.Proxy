﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Pdelvo.Async.Extensions;
using Pdelvo.Minecraft.Network;
using Pdelvo.Minecraft.Proxy.Library.Connection;
using Pdelvo.Minecraft.Proxy.Library.Plugins;

namespace Pdelvo.Minecraft.Proxy.Library
{
    public class ProxyServer : IProxyServer
    {
        IPEndPoint _localEndPoint;
        bool _listening;
        bool _acceptingNewClients;
        SynchronizedCollection<ProxyConnection> _openConnection;
        SynchronizedCollection<ProxyConnection> _connectedUsers;
        Socket _listeningSocket;
        PluginManager _pluginManager;
        ILog _logger;
        public bool IsOnlineModeEnabled { get; set; }
        public RSAParameters RSAKeyPair { get; private set; }
        public RSACryptoServiceProvider RSACryptoServiceProvider { get; private set; }

        public ProxyServer(IPEndPoint endPoint)
        {
            IsOnlineModeEnabled = false;
            _logger = LogManager.GetLogger("Proxy Server");
            _localEndPoint = endPoint;
            _pluginManager = new PluginManager();
            _pluginManager.LoadPlugins();
            _openConnection = new SynchronizedCollection<ProxyConnection>();
            _connectedUsers = new SynchronizedCollection<ProxyConnection>();
        }

        public IPEndPoint LocalEndPoint
        {
            get { return _localEndPoint; }
        }

        public int ConnectedUsers
        {
            get { return _connectedUsers.Count; }
        }

        public int MaxUsers
        {
            get { return 100; }
        }

        public string MotD
        {
            get { return "This is a great test message!"; }
        }

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
                _connectedUsers.Remove(proxyConnection);
        }

        internal void PromoteConnection(ProxyConnection proxyConnection)
        {
            _connectedUsers.Add(proxyConnection);
        }


        public IPEndPoint GetServerEndPoint(IProxyConnection proxyConnection)
        {
            return new IPEndPoint(IPAddress.Loopback, 25566);
        }
    }
}
