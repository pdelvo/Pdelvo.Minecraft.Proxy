using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    public abstract class PluginBase
    {
        public abstract string Name { get; }
        public abstract Version Version { get; }
        public abstract string Author { get; }


        public virtual bool? AllowJoining(IPAddress address)
        {
            return true;
        }
    }
}
