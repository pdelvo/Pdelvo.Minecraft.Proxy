using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Connection;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    public class UserEventArgs : CancelEventArgs
    {
        public IProxyConnection Connection { get; private set; }

        public UserEventArgs(IProxyConnection connection)
        {
            Connection = connection;
        }
    }
}
