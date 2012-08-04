using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library
{
    public interface IProxyServer : IDisposable
    {
        IPEndPoint LocalEndPoint { get; }
        int ConnectedUsers { get; }
        bool Listening { get; }
        void Start();
        Task StopAsync();
    }
}
