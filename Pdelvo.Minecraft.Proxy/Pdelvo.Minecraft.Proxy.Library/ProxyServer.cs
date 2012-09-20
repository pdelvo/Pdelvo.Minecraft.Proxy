﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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
    /// <summary>
    /// The base implementation of a proxy server.
    /// </summary>
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

        /// <summary>
        /// True if the proxy server accepts new clients, otherwise false.
        /// </summary>
        public bool AcceptingNewClients { get { return _acceptingNewClients; } }

        /// <summary>
        /// Creates a new instance of the ProxyServer class.
        /// </summary>
        public ProxyServer()
        {
            _logger = LogManager.GetLogger("Proxy Server");

            _logger.Info("Starting MineProxy.Net " + Assembly.GetExecutingAssembly().GetName().Version);

            ReadConfig();
            _pluginManager = new PluginManager(this);
            _openConnection = new SynchronizedCollection<ProxyConnection>();
            _connectedUsers = new SynchronizedCollection<ProxyConnection>();
        }

        /// <summary>
        /// Get the local end point of this proxy server where it is listening for new clients.
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }

        /// <summary>
        /// Get the number of connected proxy users.
        /// </summary>
        public int ConnectedUsers
        {
            get { return _connectedUsers.Count; }
        }

        /// <summary>
        /// Get the amount of users who can connect at the same time.
        /// </summary>
        public int MaxUsers { get; set; }

        /// <summary>
        /// Get or set if the proxy server should verify users.
        /// </summary>
        public bool OnlineMode { get; set; }

        /// <summary>
        /// Get or set the current message of the day.
        /// </summary>
        public string MotD { get; set; }

        /// <summary>
        /// Get if the underlying socket is listening for new users.
        /// </summary>
        public bool Listening
        {
            get { return _listening; }
        }

        /// <summary>
        /// Start listening for new users.
        /// </summary>
        public void Start()
        {
            if (_listening) throw new InvalidOperationException("Proxy server is running");

            _listening = true;

            RSACryptoServiceProvider rsa;

            RSAKeyPair = ProtocolSecurity.GenerateRsaKeyPair(out rsa);

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
                    CheckIPEventArgs args = new CheckIPEventArgs(address, true);

                    PluginManager.TriggerPlugin.AllowJoining(args);

                    if (!args.AllowJoining)
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

        /// <summary>
        /// Get a collection of open connections
        /// </summary>
        public IEnumerable<IProxyConnection> OpenConnections
        {
            get
            {
                return _openConnection;
            }
        }

        /// <summary>
        /// Close all active connections and free resources asynchronously
        /// </summary>
        /// <returns>A task which can be used to observe the closing process</returns>
        public async Task StopAsync()
        {
            if (!_listening) return;
            _listening = false;
            _acceptingNewClients = false;
            await Task.WhenAll(_openConnection.Select(a => a.CloseAsync()));

            _listeningSocket.Close();
            _listeningSocket.Dispose();
        }

        /// <summary>
        /// Close all active connections and free resources synchronously
        /// </summary>
        public virtual void Dispose()
        {
            StopAsync().Wait();
        }

        /// <summary>
        /// Get the current PluginManager
        /// </summary>
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


        /// <summary>
        /// Get a new server end point for a given proxy connection to.
        /// </summary>
        /// <param name="proxyConnection">The proxy connection which need a new server connection.</param>
        /// <returns>A RemoteServerInfo object which contains important information about the new backend server.</returns>
        public RemoteServerInfo GetServerEndPoint(IProxyConnection proxyConnection)
        {

            var settings = ProxyConfigurationSection.Settings;

            var server = settings.Server.OfType<ServerElement>().Where(m => m.IsDefault || (!string.IsNullOrEmpty(m.DnsName) && proxyConnection.Host.StartsWith(m.DnsName))).OrderBy(m => m.IsDefault);

            var possibleResult = server.FirstOrDefault();
            var result = possibleResult == null ? null : new RemoteServerInfo(possibleResult.Name, Extensions.ParseEndPoint(possibleResult.EndPoint), possibleResult.MinecraftVersion);

            PluginResultEventArgs<RemoteServerInfo> args = new PluginResultEventArgs<RemoteServerInfo>(result, proxyConnection);
            PluginManager.TriggerPlugin.OnPlayerServerSelection(args);
            args.EnsureSuccess();
            return args.Result;
        }

        /// <summary>
        /// Returns true if the online mode is enabled for a specific user or not.
        /// </summary>
        /// <param name="proxyConnection">The proxy connection which should be checked</param>
        /// <returns>true if the online mode is enabled, otherwise false.</returns>
        public bool OnlineModeEnabled(IProxyConnection proxyConnection)
        {
            PluginResultEventArgs<bool?> args = new PluginResultEventArgs<bool?>(null, proxyConnection);
            PluginManager.TriggerPlugin.IsOnlineModeEnabled(args);


            if (args.Result == null)
                return OnlineMode;
            return (bool)args.Result;
        }

        /// <summary>
        /// checks if a user account is valid using plugins and the minecraft.net services
        /// </summary>
        /// <param name="proxyConnection">The proxy server which should be checked.</param>
        /// <param name="hash">The 'serverId' hash</param>
        /// <returns>true if the user account is okay, otherwise false</returns>
        public async Task<bool> CheckUserAccountAsync(ProxyConnection proxyConnection, string hash)
        {
            CheckAccountEventArgs args = new CheckAccountEventArgs(null, hash, proxyConnection);

            await PluginManager.TriggerPlugin.OnUserAccountCheckAsync(args);

            args.EnsureSuccess();

            if (args.Result == null)
            {
                try
                {
                    var result = await UserAccountServices.CheckAccountAsync(proxyConnection.Username, hash);

                    if (result.HasValue) return result.Value;
                    _logger.Error("Could not access minecraft.net");
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.Error("minecraft.net not accessible", ex);

                    return false;
                }
            }
            return (bool)args.Result;
        }

        /// <summary>
        /// Get the minecraft protocol server of a given minecraft server.
        /// </summary>
        /// <param name="proxyConnection">The connection this server relates to.</param>
        /// <param name="serverEndPoint">The current version of the Remote server info.</param>
        public void GetServerVersion(IProxyConnection proxyConnection, RemoteServerInfo serverEndPoint)
        {
            PluginResultEventArgs<RemoteServerInfo> args = new PluginResultEventArgs<RemoteServerInfo>(serverEndPoint, proxyConnection);

            PluginManager.TriggerPlugin.GetServerVersion(args);

            args.EnsureSuccess();

            if (serverEndPoint.MinecraftVersion == 0)
            {
                //Look up configuration

                var settings = ProxyConfigurationSection.Settings;

                var serverList = settings.Server.OfType<ServerElement>().Where(m => m.EndPoint == serverEndPoint.EndPoint.ToString());

                var server = serverList.FirstOrDefault();

                if (server != null) serverEndPoint.MinecraftVersion = server.MinecraftVersion;
                //Use default
                else serverEndPoint.MinecraftVersion = Pdelvo.Minecraft.Protocol.Helper.ProtocolInformation.MaxSupportedServerVersion;
            }
        }
    }
}