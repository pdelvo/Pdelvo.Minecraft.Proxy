using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Connection;
using Pdelvo.Minecraft.Proxy.Library.Plugins;

namespace Pdelvo.Minecraft.Proxy.Library
{
    public interface IProxyServer : IDisposable
    {
        PluginManager PluginManager { get;}
        IPEndPoint LocalEndPoint { get; }
        int ConnectedUsers { get; }
        bool IsOnlineModeEnabled { get; }
        RSAParameters RSAKeyPair { get; }
        RSACryptoServiceProvider RSACryptoServiceProvider { get; }
        int MaxUsers { get; }
        string MotD { get; }
        bool Listening { get; }
        void Start();
        Task StopAsync();
        RemoteServerInfo GetServerEndPoint(IProxyConnection proxyConnection);
        IEnumerable<IProxyConnection> OpenConnections { get; }
    }
}
