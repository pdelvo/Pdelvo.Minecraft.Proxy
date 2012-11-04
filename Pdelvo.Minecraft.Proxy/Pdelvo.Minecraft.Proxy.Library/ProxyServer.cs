using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Pdelvo.Async.Extensions;
using Pdelvo.Minecraft.Network;
using Pdelvo.Minecraft.Protocol.Helper;
using Pdelvo.Minecraft.Proxy.Library.Configuration;
using Pdelvo.Minecraft.Proxy.Library.Connection;
using Pdelvo.Minecraft.Proxy.Library.Plugins;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;
using log4net;
using System.Collections.Concurrent;
using System.Threading;

namespace Pdelvo.Minecraft.Proxy.Library
{
    /// <summary>
    ///   The base implementation of a proxy server.
    /// </summary>
    public class ProxyServer : IProxyServer
    {
        private readonly SynchronizedCollection<ProxyConnection> _connectedUsers;
        private readonly ILog _logger;
        private readonly SynchronizedCollection<ProxyConnection> _openConnection;
        private readonly PluginManager _pluginManager;
        private bool _acceptingNewClients;
        private bool _listening;

        private List<Socket> _listeningSockets = new List<Socket>(); 

        /// <summary>
        ///   Creates a new instance of the ProxyServer class.
        /// </summary>
        public ProxyServer()
        {
            _logger = LogManager.GetLogger("Proxy Server");

            _logger.Info("Starting MineProxy.Net " + Assembly.GetExecutingAssembly ().GetName ().Version);

            ReadConfig ();
            _pluginManager = new PluginManager(this);
            _openConnection = new SynchronizedCollection<ProxyConnection> ();
            _connectedUsers = new SynchronizedCollection<ProxyConnection> ();
        }

        internal RSAParameters RSAKeyPair { get; private set; }
        internal RSACryptoServiceProvider RSACryptoServiceProvider { get; private set; }

        /// <summary>
        ///   True if the proxy server accepts new clients, otherwise false.
        /// </summary>
        public bool AcceptingNewClients
        {
            get { return _acceptingNewClients; }
        }

        #region IProxyServer Members

        /// <summary>
        ///   Get the number of connected proxy users.
        /// </summary>
        public int ConnectedUsers
        {
            get { return _connectedUsers.Count; }
        }

        public ProxyConfigurationSection Configuration { get; private set; }

        /// <summary>
        ///   Get if the underlying socket is listening for new users.
        /// </summary>
        public bool Listening
        {
            get { return _listening; }
        }

        /// <summary>
        ///   Start listening for new users.
        /// </summary>
        public void Start()
        {
            if (_listening) throw new InvalidOperationException("Proxy server is running");

            _listening = true;

            RSACryptoServiceProvider rsa;

            RSAKeyPair = ProtocolSecurity.GenerateRsaKeyPair(out rsa);

            RSACryptoServiceProvider = rsa;

            _logger.Info("RSA Key pair generated");
            foreach (EndPointElement endPoint in Configuration.EndPoints)
            {
                AddListeningSocket(endPoint);
            }
        }

        public void AddListeningSocket(EndPointElement element)
        {
            var localEndPoint = Extensions.ParseEndPoint(element.LocalEndPoint);

            var listeningSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listeningSocket.Bind(localEndPoint);

            _logger.InfoFormat("Server is listening at {0}.", listeningSocket);

            listeningSocket.Listen(10);

            _listeningSockets.Add(listeningSocket);
            Task.Run(() => ReceiveClientsAsync(listeningSocket));

        }

        /// <summary>
        ///   Get a collection of open connections
        /// </summary>
        public IEnumerable<IProxyConnection> OpenConnections
        {
            get { return _openConnection; }
        }

        /// <summary>
        ///   Close all active connections and free resources asynchronously
        /// </summary>
        /// <returns> A task which can be used to observe the closing process </returns>
        public async Task StopAsync()
        {
            if (!_listening) return;
            _listening = false;
            _acceptingNewClients = false;
            await Task.WhenAll(_openConnection.Select(a => a.CloseAsync ()));

            foreach (var listeningSocket in _listeningSockets)
            {
                CloseSocket(listeningSocket);
            }
        }

        private void CloseSocket(Socket listeningSocket)
        {
            listeningSocket.Close();
            listeningSocket.Dispose();

            _listeningSockets.Remove(listeningSocket);
        }

        /// <summary>
        ///   Close all active connections and free resources synchronously
        /// </summary>
        public virtual void Dispose()
        {
            StopAsync ().Wait ();
        }

        /// <summary>
        ///   Get the current PluginManager
        /// </summary>
        public PluginManager PluginManager
        {
            get { return _pluginManager; }
        }

        /// <summary>
        ///   Get a new server end point for a given proxy connection to.
        /// </summary>
        /// <param name="proxyConnection"> The proxy connection which need a new server connection. </param>
        /// <returns> A RemoteServerInfo object which contains important information about the new backend server. </returns>
        public RemoteServerInfo GetServerEndPoint(IProxyConnection proxyConnection)
        {
            ProxyConfigurationSection settings = Configuration;

            IOrderedEnumerable<ServerElement> server =
                settings.Server.OfType<ServerElement> ().Where(
                    m =>  GetServerConfiguration(((ProxyEndPoint)proxyConnection.ClientEndPoint).LocalEndPoint).BackEndRefs.Cast<NamedConfigurationElement> ().Any(a=>a.Name == m.Name) &&
                        (m.IsDefault || (!string.IsNullOrEmpty(m.DnsName) && proxyConnection.Host.StartsWith(m.DnsName))))
                    .OrderBy(m => m.IsDefault);

            ServerElement possibleResult = server.FirstOrDefault ();
            RemoteServerInfo result = possibleResult == null
                                          ? null
                                          : new RemoteServerInfo(possibleResult.Name,
                                                                 Extensions.ParseEndPoint(possibleResult.EndPoint),
                                                                 possibleResult.MinecraftVersion)
                                                {
                                                    KickMessage = possibleResult.KickMessage
                                                };

            var args = new PluginResultEventArgs<RemoteServerInfo>(result, proxyConnection);
            PluginManager.TriggerPlugin.OnPlayerServerSelection(args);
            args.EnsureSuccess ();
            return args.Result;
        }

        #endregion

        private void ReadConfig()
        {
            Configuration = ProxyConfigurationSection.Settings;
        }

        private async void ReceiveClientsAsync(Socket listeningSocket)
        {
            _acceptingNewClients = true;
            while (true)
            {
                try
                {
                    Socket remote = await listeningSocket.AcceptTaskAsync();

                    IPAddress address = ((IPEndPoint) remote.RemoteEndPoint).Address;
                    var args = new CheckIPEventArgs(address, true);

                    PluginManager.TriggerPlugin.AllowJoining(args);

                    if (!args.AllowJoining)
                    {
                        remote.Close ();
                        _logger.Warn("Denied access from " + address);
                        continue;
                    }
                    //Connection accepted

                    var proxyConnection = new ProxyConnection(remote, this);

                    _openConnection.Add(proxyConnection);


                    proxyConnection.HandleClient ();
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
        ///   Returns true if the online mode is enabled for a specific user or not.
        /// </summary>
        /// <param name="proxyConnection"> The proxy connection which should be checked </param>
        /// <returns> true if the online mode is enabled, otherwise false. </returns>
        public bool OnlineModeEnabled(IProxyConnection proxyConnection)
        {
            var args = new PluginResultEventArgs<bool?>(null, proxyConnection);
            PluginManager.TriggerPlugin.IsOnlineModeEnabled(args);


            if (args.Result == null)
                return GetServerConfiguration(((ProxyEndPoint)proxyConnection.ClientEndPoint).LocalEndPoint).OnlineMode;
            return (bool) args.Result;
        }

        /// <summary>
        ///   checks if a user account is valid using plugins and the minecraft.net services
        /// </summary>
        /// <param name="proxyConnection"> The proxy server which should be checked. </param>
        /// <param name="hash"> The 'serverId' hash </param>
        /// <returns> true if the user account is okay, otherwise false </returns>
        public async Task<bool> CheckUserAccountAsync(ProxyConnection proxyConnection, string hash)
        {
            var args = new CheckAccountEventArgs(null, hash, proxyConnection);

            await PluginManager.TriggerPlugin.OnUserAccountCheckAsync(args);

            args.EnsureSuccess ();

            if (args.Result == null)
            {
                try
                {
                    bool? result = await UserAccountServices.CheckAccountAsync(proxyConnection.Username, hash);

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
            return (bool) args.Result;
        }

        /// <summary>
        ///   Get the minecraft protocol server of a given minecraft server.
        /// </summary>
        /// <param name="proxyConnection"> The connection this server relates to. </param>
        /// <param name="serverEndPoint"> The current version of the Remote server info. </param>
        public void GetServerVersion(IProxyConnection proxyConnection, RemoteServerInfo serverEndPoint)
        {
            var args = new PluginResultEventArgs<RemoteServerInfo>(serverEndPoint, proxyConnection);

            PluginManager.TriggerPlugin.GetServerVersion(args);

            args.EnsureSuccess ();

            if (serverEndPoint.MinecraftVersion == 0)
            {
                //Look up configuration

                ProxyConfigurationSection settings = ProxyConfigurationSection.Settings;

                IEnumerable<ServerElement> serverList =
                    settings.Server.OfType<ServerElement> ().Where(
                        m => m.EndPoint == serverEndPoint.EndPoint.ToString ());

                ServerElement server = serverList.FirstOrDefault ();

                if (server != null) serverEndPoint.MinecraftVersion = server.MinecraftVersion;
                    //Use default
                else serverEndPoint.MinecraftVersion = ProtocolInformation.MaxSupportedServerVersion;
            }
        }

        public EndPointElement GetServerConfiguration(IPEndPoint localEndPoint)
        {
            return Configuration.EndPoints.Cast<EndPointElement> ().Single(a => EndPointMatch(localEndPoint, Extensions.ParseEndPoint(a.LocalEndPoint)));
        }

        private bool EndPointMatch(IPEndPoint localEndPoint, IPEndPoint iPEndPoint)
        {
            if(localEndPoint.AddressFamily == iPEndPoint.AddressFamily)
            {
                if(localEndPoint.Address.Equals(iPEndPoint.Address) ||
                    localEndPoint.Address.Equals(IPAddress.Any) ||
                    iPEndPoint.Address.Equals(IPAddress.Any) ||
                    localEndPoint.Address.Equals(IPAddress.IPv6Any) ||
                    iPEndPoint.Address.Equals(IPAddress.IPv6Any))
                {
                    return localEndPoint.Port == iPEndPoint.Port;
                }
            }
            return false;
        }
    }
}