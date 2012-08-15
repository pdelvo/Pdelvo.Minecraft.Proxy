using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Connection;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    public class CheckAccountEventArgs : PluginResultEventArgs<bool?>
    {
        public string Hash { get; set; }

        public CheckAccountEventArgs(bool? startResult, string hash, IProxyConnection connection)
            :base(startResult, connection)
        {
            Hash = hash;
        }
    }
}
