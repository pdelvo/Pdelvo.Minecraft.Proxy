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

        public string Username { get; protected set; }
        public string Host { get; protected set; }
        public int EntityID { get; protected set; }

        public ProxyConnection(Socket networkSocket, ProxyServer server)
        {
            _logger = LogManager.GetLogger("Proxy Connection");
            _networkSocket = networkSocket;
            _server = server;
            _random = new Random();
        }

        public async Task CloseAsync()
        {
            await KickUserAsync("Proxy connection shutdown");
            if (_serverEndPoint != null)
                _serverEndPoint.Close();
        }

        public void Dispose()
        {

        }

        internal virtual async void HandleClient()
        {
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
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error("Failed to login a Client", ex);
            }
            if (!success)
                await KickUserAsync("Failed to login");
        }

        private async Task<Packet> InitializeServerAsync()
        {
            bool success = true;
            try
            {
                return await InitializeServerAsync(_server.GetServerEndPoint(this));
            }
            catch (Exception)
            {
                success = false;
            }
            if (!success)
                await KickUserAsync("Could not connect to remote server");
            throw new TaskCanceledException();
        }

        public async Task<Packet> InitializeServerAsync(RemoteServerInfo serverEndPoint)
        {
            ProxyEndPoint server = null;
            try
            {
                UnregisterServer();
                _serverEndPoint = null;

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

                var encryptionKeyRequest = await server.ReceivePacketAsync() as EncryptionKeyRequest;

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
                _logger.Error("Could not connect to remote server", ex);
                throw;
            }
            finally
            {
                _serverEndPoint = server;
            }
        }

        public void StartServerListening()
        {
            ServerEndPoint.ConnectionLost += ServerConnectionLost;
            ServerEndPoint.PacketReceived += ServerPacketReceived;
            ServerEndPoint.StartListening();
        }

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

        void ClientPacketReceived(object sender, PacketReceivedEventArgs args)
        {
            args.Connection = this;
            _server.PluginManager.ApplyClientPacket(args);

            if (!args.Handled)
                ServerEndPoint.SendPacketQueued(args.Packet);
        }

        void ServerPacketReceived(object sender, PacketReceivedEventArgs args)
        {
            args.Connection = this;
            _server.PluginManager.ApplyServerPacket(args);

            if (!args.Handled)
                ClientEndPoint.SendPacketQueued(args.Packet);
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

        public IProxyEndPoint ServerEndPoint
        {
            get { return _serverEndPoint; }
        }

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
