using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Connection;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    public class PlayerConnectedEventArgs : CancelEventArgs
    {
        public IProxyConnection Connection { get; private set; }

        public PlayerConnectedEventArgs(IProxyConnection connection)
        {
            Connection = connection;
        }
    }
}
