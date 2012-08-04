using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library
{
    public class ProxyServer : IProxyServer
    {

        public IPEndPoint LocalEndPoint
        {
            get { throw new NotImplementedException(); }
        }

        public int ConnectedUsers
        {
            get { throw new NotImplementedException(); }
        }

        public bool Listening
        {
            get { throw new NotImplementedException(); }
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {

        }
    }
}
