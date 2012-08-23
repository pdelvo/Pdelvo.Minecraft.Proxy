using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Pdelvo.Minecraft.Network;
using Pdelvo.Minecraft.Protocol;
using Pdelvo.Minecraft.Protocol.Helper;
using Pdelvo.Minecraft.Protocol.Packets;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;
using Pdelvo.Async.Extensions;
using System.Net;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    /// <summary>
    /// The basic implementation of the IProxyConnection interface
    /// </summary>
    public class ProxyConnection : IProxyConnection
    {
        Socket _networkSocket;
        ProxyServer _server;
        ProxyEndPoint _serverEndPoint;
        ProxyEndPoint _clientEndPoint;
        ILog _logger;
        Random _random;
        bool _connectionClosed;
        bool _quitMessagePosted;
        bool _isMotDRequest;

        /// <summary>
        /// Get the username of the current user
        /// </summary>
        public string Username { get; protected set; }

        /// <summary>
        /// Get the Hostname and port the user used to connect to the server
        /// </summary>
        public string Host { get; protected set; }

        /// <summary>
        /// Get the current entity id of the user
        /// </summary>
        public int EntityID { get; protected set; }

        /// <summary>
        /// Creates a new instance of the ProxyConnection class with the remote socket of the client 
        /// and the ProxyServer this connection should belong to.
        /// </summary>
        /// <param name="networkSocket">The network socket of the network client</param>
        /// <param name="server">The proxy server this connection belongs to</param>
        public ProxyConnection(Socket networkSocket, ProxyServer server)
        {
            _logger = LogManager.GetLogger("Proxy Connection");
            _networkSocket = networkSocket;
            _server = server;
            _random = new Random();
        }

        /// <summary>
        /// Asynchronously close this connection
        /// </summary>
        /// <returns>A task representating the closing process</returns>
        public async Task CloseAsync()
        {
            await KickUserAsync("Proxy connection shutdown");
            if (_serverEndPoint != null)
            {
                _serverEndPoint.Close();
                _serverEndPoint = null;
            }
        }

        /// <summary>
        /// Cleans all resources of this connection
        /// </summary>
        public async void Dispose()
        {
            await CloseAsync();
        }

        internal virtual async void HandleClient()
        {
            var kickMessage = "Failed to login";
            bool success = true;
            try
            {
                var clientRemoteInterface = ClientRemoteInterface.Create(new NetworkStream(_networkSocket), 39);
                _clientEndPoint = new ProxyEndPoint(clientRemoteInterface, clientRemoteInterface.EndPoint.Version);
                _clientEndPoint.RemoteEndPoint = (IPEndPoint)_networkSocket.RemoteEndPoint;

                var packet = await clientRemoteInterface.ReadPacketAsync();

                var listPing = packet as PlayerListPing;
                var handshakeRequest = packet as HandshakeRequest;


                if (listPing != null) // send motd
                {
                    _isMotDRequest = true;
                    var response = ProtocolHelper.BuildMotDString(_server.MotD, _server.ConnectedUsers, _server.MaxUsers);
                    await KickUserAsync(response);
                    return;
                }

                if (handshakeRequest != null)
                {
                    Username = handshakeRequest.UserName;
                    Host = handshakeRequest.Host;
                    ClientEndPoint.ProtocolVersion = handshakeRequest.ProtocolVersion;

                    if (handshakeRequest.ProtocolVersion < ProtocolInformation.MinSupportedClientVersion)
                    {
                        await KickUserAsync("Outdated Client");
                        return;
                    }
                    else if (handshakeRequest.ProtocolVersion > ProtocolInformation.MaxSupportedClientVersion)
                    {
                        await KickUserAsync("Outdated Server");
                        return;
                    }

                    var args = new UserEventArgs(this);

                    _server.PluginManager.TriggerPlugin.OnPlayerConnected(args);

                    if (args.Canceled)
                    {
                        await ClientEndPoint.SendPacketAsync(new DisconnectPacket { Reason = args.CancelMessage });
                        _networkSocket.Close();
                        _server.RemoveConnection(this);
                        return;
                    }

                    bool onlineMode = _server.OnlineModeEnabled(this);
                    string serverId = onlineMode ? Session.GetSessionHash() : "-";

                    byte[] randomBuffer = new byte[4];
                    _random.NextBytes(randomBuffer);
                    await ClientEndPoint.SendPacketAsync(new EncryptionKeyRequest
                    {
                        ServerId = serverId,
                        PublicKey = AsnKeyBuilder.PublicKeyToX509(_server.RSAKeyPair).GetBytes(),
                        VerifyToken = randomBuffer
                    });
                    do
                    {
                        packet = await ClientEndPoint.ReceivePacketAsync();
                    } while (packet is KeepAlive);

                    var encryptionKeyResponse = (EncryptionKeyResponse)packet;
                    var verification = Pdelvo.Minecraft.Network.ProtocolSecurity.RSADecrypt(
                        encryptionKeyResponse.VerifyToken.ToArray(), _server.RSACryptoServiceProvider, true);
                    var sharedKey = Pdelvo.Minecraft.Network.ProtocolSecurity.RSADecrypt(
                        encryptionKeyResponse.SharedKey.ToArray(), _server.RSACryptoServiceProvider, true);
                    if (verification.Length != randomBuffer.Length
                        || !verification.Zip(randomBuffer, (a, b) => a == b).All(a => a))
                    {
                        await KickUserAsync("Verify token failure");
                        _logger.Error("Failed to login a Client, Verify token failure");
                        return;
                    }

                    await ClientEndPoint.SendPacketAsync(new EncryptionKeyResponse { SharedKey = new byte[0] });

                    ClientEndPoint.ConnectionKey = sharedKey;
                    ClientEndPoint.EnableAes();//now everything is encrypted

                    var p = await ClientEndPoint.ReceivePacketAsync();

                    if (!(p is RespawnRequestPacket))
                    {
                        await KickUserAsync("Protocol failure");
                        _logger.Error("Failed to login a Client, Protocol failure");
                        return;
                    }

                    var hash = ProtocolSecurity.ComputeHash(
                        Encoding.ASCII.GetBytes(serverId),
                        ClientEndPoint.ConnectionKey,
                        AsnKeyBuilder.PublicKeyToX509(_server.RSAKeyPair).GetBytes());

                    if (onlineMode)
                    {
                        bool result;
                        try
                        {
                            result = await _server.CheckUserAccountAsync(this, hash);
                        }
                        catch (OperationCanceledException ex)
                        {
                            var t = KickUserAsync(ex.Message);
                            return;
                        }

                        if (!result)
                        {
                            await KickUserAsync("User not premium");
                            return;
                        }
                    }

                    _logger.InfoFormat("{0}[{1}] is connected", Username, _networkSocket.RemoteEndPoint);

                    _server.PromoteConnection(this);

                    var response = await InitializeServerAsync();

                    var logonResponse = response as LogOnResponse;
                    if (logonResponse != null)
                    {
                        EntityID = logonResponse.EntityId;
                    }

                    await ClientEndPoint.SendPacketAsync(response);

                    StartClientListening();
                    StartServerListening();

                    args = new UserEventArgs(this);

                    _server.PluginManager.TriggerPlugin.OnUserConnectedCompleted(args);

                    args.EnsureSuccess();
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (OperationCanceledException ex)
            {
                kickMessage = ex.Message;
                success = false;
                _quitMessagePosted = true;
                _logger.Error(string.Format("Client login aborted ({0})", Username), ex);
            }
            catch (Exception ex)
            {
                success = false;
                _quitMessagePosted = true;
                _logger.Error(string.Format("Failed to login a client ({0})", Username), ex);
            }
            if (!success)
                await KickUserAsync(kickMessage);
        }

        private async Task<Packet> InitializeServerAsync()
        {
            bool success = true;
            RemoteServerInfo info = null;
            try
            {
                info = _server.GetServerEndPoint(this);
            }
            catch (Exception ex)
            {
                _quitMessagePosted = true;

                _logger.Error("Could not get remote server info", ex);

                success = false;
            }
            if (success)
            {
                try
                {
                    return await InitializeServerAsync(info);
                }
                catch (Exception ex)
                {
                    _quitMessagePosted = true;

                    _logger.Error(string.Format("Could not connect to remote server ({0})", info.EndPoint), ex);

                    success = false;
                }
            }
            if (!success)
                await KickUserAsync("Could not connect to remote server");
            throw new TaskCanceledException();
        }

        /// <summary>
        /// Asynchronously initialize the server side of this connection
        /// </summary>
        /// <param name="serverEndPoint">Information of the new server this connection should connect to.</param>
        /// <returns>A task which returns the LogOnPacket or DisconnectPacket of the established connection.</returns>
        public async Task<Packet> InitializeServerAsync(RemoteServerInfo serverEndPoint)
        {
            ProxyEndPoint server = null;
            try
            {
                UnregisterServer();
                _serverEndPoint = null;

                if(serverEndPoint.MinecraftVersion == 0)
                _server.GetServerVersion(this, serverEndPoint);

                var socket = new Socket(serverEndPoint.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = 1024 * 1024
                };

                await socket.ConnectTaskAsync(serverEndPoint.EndPoint);


                server = new ProxyEndPoint(ServerRemoteInterface.Create(new NetworkStream(socket), serverEndPoint.MinecraftVersion), serverEndPoint.MinecraftVersion);
                server.RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
                var handshakeRequest = new HandshakeRequest
                {
                    UserName = Username,
                    Host = Host,
                    ProtocolVersion = (byte)serverEndPoint.MinecraftVersion
                };
                await server.SendPacketAsync(handshakeRequest);

                Packet tp = await server.ReceivePacketAsync();

                if (tp is DisconnectPacket)
                {
                    throw new OperationCanceledException((tp as DisconnectPacket).Reason);
                }

                var encryptionKeyRequest = tp as EncryptionKeyRequest;

                server.ConnectionKey = ProtocolSecurity.GenerateAes128Key();
                byte[] key = Pdelvo.Minecraft.Network.ProtocolSecurity.RSAEncrypt(server.ConnectionKey, encryptionKeyRequest.PublicKey.ToArray(), false);
                byte[] verifyToken = Pdelvo.Minecraft.Network.ProtocolSecurity.RSAEncrypt(encryptionKeyRequest.VerifyToken.ToArray(), encryptionKeyRequest.PublicKey.ToArray(), false);

                var encryptionKeyResponse = new EncryptionKeyResponse
                {
                    SharedKey = key,
                    VerifyToken = verifyToken
                };
                await server.SendPacketAsync(encryptionKeyResponse);

                var p = await server.ReceivePacketAsync();

                server.EnableAes();

                await server.SendPacketAsync(new RespawnRequestPacket());

                return await server.ReceivePacketAsync();
            }
            catch (Exception ex)
            {
                _quitMessagePosted = true;
                _logger.Error("Could not connect to remote server", ex);
                throw;
            }
            finally
            {
                _serverEndPoint = server;
            }
        }

        /// <summary>
        /// Start waiting for server packets
        /// </summary>
        public void StartServerListening()
        {
            ServerEndPoint.ConnectionLost += ServerConnectionLost;
            ServerEndPoint.PacketReceived += ServerPacketReceived;
            ServerEndPoint.StartListening();
        }

        /// <summary>
        /// Start waiting for client packets
        /// </summary>
        public void StartClientListening()
        {
            ClientEndPoint.ConnectionLost += ClientConnectionLost;
            ClientEndPoint.PacketReceived += ClientPacketReceived;
            ClientEndPoint.StartListening();
        }

        void ClientConnectionLost(object sender, EventArgs e)
        {
            OnConnectionLost();
        }

        private void OnConnectionLost()
        {
            if (_connectionClosed) return;
            _connectionClosed = true;
            ServerEndPoint.Close();
            ClientEndPoint.Close();
            if (!_quitMessagePosted)
            {
                _logger.Info(Username + " lost connection");
                _quitMessagePosted = true;
            }
            _server.RemoveConnection(this);
        }

        void ServerConnectionLost(object sender, EventArgs e)
        {
            OnConnectionLost();
        }

        async void ClientPacketReceived(object sender, PacketReceivedEventArgs args)
        {
            string kickMessage = null;
            try
            {
                args.Connection = this;
                _server.PluginManager.ApplyClientPacket(args);

                if (!args.Handled)
                    ServerEndPoint.SendPacketQueued(args.Packet);
            }
            catch (OperationCanceledException ex)
            {
                kickMessage = ex.Message;
            }
            if (kickMessage != null)
                await KickUserAsync(kickMessage);
        }

        async void ServerPacketReceived(object sender, PacketReceivedEventArgs args)
        {
            string kickMessage = null;
            try
            {
                args.Connection = this;
                _server.PluginManager.ApplyServerPacket(args);

                if (!args.Handled)
                    ClientEndPoint.SendPacketQueued(args.Packet);
            }
            catch (OperationCanceledException ex)
            {
                kickMessage = ex.Message;
            }
            if (kickMessage != null)
                await KickUserAsync(kickMessage);
        }

        private async Task KickUserAsync(string message)
        {
            try
            {
                await ClientEndPoint.SendPacketAsync(new DisconnectPacket { Reason = message });
                if (!_quitMessagePosted &&!_isMotDRequest )
                {
                    _logger.Info(Username + " lost connection, message: " + message);
                    _quitMessagePosted = true;
                }
                ClientEndPoint.Close();
                if (ServerEndPoint != null)
                {
                    ServerEndPoint.Close();
                }
            }
            catch (Exception) { }
            finally
            {
                _server.RemoveConnection(this);
            }
        }

        /// <summary>
        /// Get the current server end point
        /// </summary>
        public IProxyEndPoint ServerEndPoint
        {
            get { return _serverEndPoint; }
        }

        /// <summary>
        /// Get the current client end point
        /// </summary>
        public IProxyEndPoint ClientEndPoint
        {
            get { return _clientEndPoint; }
        }


        void UnregisterServer()
        {
            if (ServerEndPoint != null)
            {
                ServerEndPoint.PacketReceived -= ServerPacketReceived;
                ServerEndPoint.ConnectionLost -= ServerConnectionLost;
                ServerEndPoint.Close();
                _serverEndPoint = null;
            }
        }
    }
}
