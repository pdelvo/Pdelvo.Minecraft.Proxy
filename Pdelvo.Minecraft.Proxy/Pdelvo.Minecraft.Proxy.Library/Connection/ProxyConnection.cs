﻿using System;
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

        public string Username { get; protected set; }
        public string Host { get; protected set; }

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


                var packet = await clientRemoteInterface.ReadPacketAsync();

                var listPing = packet as PlayerListPing;
                var handshakeRequest = packet as HandshakeRequest;


                if (listPing != null) // send motd
                {
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
                    _logger.InfoFormat("{0} is connecting...", Username);

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

                    //TODO: Acc plugin support
                    if (onlineMode)
                    {
                        bool result;
                        try
                        {
                            result = await UserAccountServices.CheckAccountAsync(Username, hash);
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

                    _logger.InfoFormat("{0} is connected", Username);

                    _server.PromoteConnection(this);

                    var response = await InitializeServerAsync();

                    await ClientEndPoint.SendPacketAsync(response);

                    ClientEndPoint.PacketReceived += ClientPacketReceived;
                    ServerEndPoint.PacketReceived += ServerPacketReceived;

                    ClientEndPoint.ConnectionLost += ClientConnectionLost;
                    ServerEndPoint.ConnectionLost += ServerConnectionLost;

                    ClientEndPoint.StartListening();
                    ServerEndPoint.StartListening();
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
            try
            {
                var socket = new Socket(serverEndPoint.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = 1024*1024
                };

                await socket.ConnectTaskAsync(serverEndPoint.EndPoint);

                _serverEndPoint = new ProxyEndPoint(ServerRemoteInterface.Create(new NetworkStream(socket), ClientEndPoint.ProtocolVersion), ClientEndPoint.ProtocolVersion);

                var handshakeRequest = new HandshakeRequest
                {
                    UserName = Username,
                    Host = Host,
                    ProtocolVersion = (byte)serverEndPoint.MinecraftVersion
                };
                await ServerEndPoint.SendPacketAsync(handshakeRequest);

                var encryptionKeyRequest = await ServerEndPoint.ReceivePacketAsync() as EncryptionKeyRequest;

                ServerEndPoint.ConnectionKey = ProtocolSecurity.GenerateAes128Key();
                byte[] key = Pdelvo.Minecraft.Network.ProtocolSecurity.RSAEncrypt(ServerEndPoint.ConnectionKey, encryptionKeyRequest.PublicKey.ToArray(), false);
                byte[] verifyToken = Pdelvo.Minecraft.Network.ProtocolSecurity.RSAEncrypt(encryptionKeyRequest.VerifyToken.ToArray(), encryptionKeyRequest.PublicKey.ToArray(), false);

                var encryptionKeyResponse = new EncryptionKeyResponse
                {
                    SharedKey = key,
                    VerifyToken = verifyToken
                };
                await ServerEndPoint.SendPacketAsync(encryptionKeyResponse);

                var p = await ServerEndPoint.ReceivePacketAsync();

                ServerEndPoint.EnableAes();

                await ServerEndPoint.SendPacketAsync(new RespawnRequestPacket());

                return await ServerEndPoint.ReceivePacketAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Could not connect to remote server", ex);
                throw;
            }
        }

        void ClientConnectionLost(object sender, EventArgs e)
        {
            OnConnectionLost();
        }

        private void OnConnectionLost()
        {
            OnConnectionLost();
        }

        void ServerConnectionLost(object sender, EventArgs e)
        {
            if (_connectionClosed) return;
            _connectionClosed = true;
            ServerEndPoint.Close();
            ClientEndPoint.Close();
            _logger.Error(Username + " lost connection");
            _server.RemoveConnection(this);
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
    }
}
