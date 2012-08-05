using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    internal class TriggerPlugin : PluginBase
    {
        List<PluginBase> _triggerPlugins;

        public TriggerPlugin(List<PluginBase> triggerPlugins)
        {
            _triggerPlugins = triggerPlugins;
        }

        public override string Name
        {
            get { throw new NotSupportedException(); }
        }

        public override Version Version
        {
            get { throw new NotSupportedException(); }
        }

        public override string Author
        {
            get { throw new NotSupportedException(); }
        }

        public override bool? AllowJoining(System.Net.IPAddress address)
        {
            return _triggerPlugins.Select(a => a.AllowJoining(address)).SkipWhile(a => a == null).FirstOrDefault() ?? true;
        }
    }
}
