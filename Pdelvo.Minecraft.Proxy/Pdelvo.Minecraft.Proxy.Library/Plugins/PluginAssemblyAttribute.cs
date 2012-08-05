using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Plugins;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public sealed class PluginAssemblyAttribute : Attribute
    {
        readonly Type _pluginType;

        // This is a positional argument
        public PluginAssemblyAttribute(Type pluginType)
        {
            if(pluginType == null)throw new ArgumentNullException("pluginType");
            if (!pluginType.IsSubclassOf(typeof(PluginBase))) throw new InvalidOperationException("pluginType must be a type inhertig PluginBase");

            _pluginType = pluginType;
        }

        public Type PluginType
        {
            get { return _pluginType; }
        }
    }
}
