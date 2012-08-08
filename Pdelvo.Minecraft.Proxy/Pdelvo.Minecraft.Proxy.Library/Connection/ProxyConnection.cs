using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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

        public string Username { get; protected set; }
        public string Host { get; protected set; }

        public ProxyConnection(Socket networkSocket, ProxyServer server)
        {
            _networkSocket = networkSocket;
            _server = server;
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
                    _server.RemoteConnection(this);
                    return;
                }

                if (handshakeRequest != null)
                {
                    Username = handshakeRequest.UserName;
                    Host = handshakeRequest.Host;
                    ClientEndPoint.ProtocolVersion = handshakeRequest.ProtocolVersion;
                    var args = new PlayerConnectedEventArgs(this);


                }
            }
            catch (Exception ex)
            {

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
