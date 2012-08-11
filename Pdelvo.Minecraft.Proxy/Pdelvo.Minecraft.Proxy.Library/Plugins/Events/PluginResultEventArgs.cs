using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Connection;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    public class PluginResultEventArgs<T> : UserEventArgs
    {
        public T Result { get; set; }

        public PluginResultEventArgs(T result, IProxyConnection connection)
            :base(connection)
        {

        }
    }
}
