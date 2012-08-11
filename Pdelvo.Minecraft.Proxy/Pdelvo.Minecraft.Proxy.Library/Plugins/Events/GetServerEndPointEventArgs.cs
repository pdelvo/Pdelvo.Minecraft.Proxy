using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Connection;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    public class GetServerEndPointEventArgs : CancelEventArgs
    {
        public RemoteServerInfo CurrentInfo { get; set; }
        public IProxyConnection Connection { get; private set; }

        public GetServerEndPointEventArgs(IProxyConnection connection, RemoteServerInfo info)
        {
            Connection = connection;
            CurrentInfo = info;
        }
    }
}
