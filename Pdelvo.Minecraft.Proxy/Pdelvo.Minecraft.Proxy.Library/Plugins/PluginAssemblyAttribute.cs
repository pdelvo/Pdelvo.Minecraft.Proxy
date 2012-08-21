using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Plugins;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    /// <summary>
    /// A attribute which should be placed in  an assembly containing proxy server plugins to point the plugin loader where it can find plugins
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public sealed class PluginAssemblyAttribute : Attribute
    {
        readonly Type _pluginType;

        /// <summary>
        /// Creates a new instance of the PluginAssemblyAttribute
        /// </summary>
        /// <param name="pluginType">A type which must inherit from <see cref="PluginBase">PluginBase</see></param>
        public PluginAssemblyAttribute(Type pluginType)
        {
            if(pluginType == null)throw new ArgumentNullException("pluginType");
            if (!pluginType.IsSubclassOf(typeof(PluginBase))) throw new InvalidOperationException("pluginType must be a type inhertig PluginBase");

            _pluginType = pluginType;
        }

        /// <summary>
        /// The type of the plugin this attribute points at
        /// </summary>
        public Type PluginType
        {
            get { return _pluginType; }
        }
    }
}
