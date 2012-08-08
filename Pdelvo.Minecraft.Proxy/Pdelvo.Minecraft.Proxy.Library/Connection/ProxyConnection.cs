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

        public string Username { get; protected set; }
        public string Host { get; protected set; }

        public ProxyConnection(Socket networkSocket, ProxyServer server)
        {
            _logger = LogManager.GetLogger("Proxy Connection");
            _networkSocket = networkSocket;
            _server = server;
            _random = new Random();
        }

        public Task CloseAsync()
        {
            return Task.FromResult(0);
        }

        public void Dispose()
        {

        }

        internal virtual async void HandleClient()
        {
            try
            {
                var clientRemoteInterface = ClientRemoteInterface.Create(new NetworkStream(_networkSocket), 39);
                _clientEndPoint = new ProxyEndPoint(clientRemoteInterface, clientRemoteInterface.EndPoint.Version);

                var packet = await clientRemoteInterface.ReadPacketAsync();

                var listPing = packet as PlayerListPing;
                var handshakeRequest = packet as HandshakeRequest;


                if (listPing != null) // send motd
                {
                    var response = ProtocolHelper.BuildMotDPacket(_server.MotD, _server.ConnectedUsers, _server.MaxUsers);

                    await ClientEndPoint.SendPacketAsync(response);
                    _networkSocket.Close();
                    _server.RemoveConnection(this);
                    return;
                }

                if (handshakeRequest != null)
                {
                    Username = handshakeRequest.UserName;
                    Host = handshakeRequest.Host;
                    ClientEndPoint.ProtocolVersion = handshakeRequest.ProtocolVersion;
                    var args = new PlayerConnectedEventArgs(this);

                    _server.PluginManager.TriggerPlugin.OnPlayerConnected(args);

                    if (args.Canceled)
                    {
                        await ClientEndPoint.SendPacketAsync(new DisconnectPacket { Reason = args.CancelMessage });
                        _networkSocket.Close();
                        _server.RemoveConnection(this);
                        return;
                    }
                    _logger.InfoFormat("{0} is connecting...", Username);

                    bool onlineMode = _server.IsOnlineModeEnabled;
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
                        await ClientEndPoint.SendPacketAsync(new DisconnectPacket { Reason = "Verify token failure" });
                        _logger.Error("Failed to login a Client, Verify token failure");
                        _networkSocket.Close();
                        _server.RemoveConnection(this);
                        return;
                    }

                    await ClientEndPoint.SendPacketAsync(new EncryptionKeyResponse { SharedKey = new byte[0] });

                    ClientEndPoint.ConnectionKey = sharedKey;
                    ClientEndPoint.EnableAes();//now everything is encrypted

                    var p = await ClientEndPoint.ReceivePacketAsync();

                    if (!(p is RespawnRequestPacket))
                    {
                        await ClientEndPoint.SendPacketAsync(new DisconnectPacket { Reason = "Protocol failure" });
                        _logger.Error("Failed to login a Client, Protocol failure");
                        _networkSocket.Close();
                        _server.RemoveConnection(this);
                        return;
                    }


                    _logger.InfoFormat("{0} is connected", Username);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to login a Client", ex);
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
