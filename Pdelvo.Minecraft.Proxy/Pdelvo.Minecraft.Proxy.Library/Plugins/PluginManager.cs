using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    public class PluginManager
    {
        List<PluginBase> _plugins = new List<PluginBase>();

        ILog _logger;

        static PluginManager()
        {
            ResolveEventHandler handler = (s, e) =>
            {
                return Assembly.LoadFile(e.Name);
            };
            AppDomain.CurrentDomain.AssemblyResolve += handler;
        }

        public PluginManager()
        {
            _logger = LogManager.GetLogger("Plugin Manager");
            TriggerPlugin = new TriggerPlugin(_plugins);

            _plugins.AddRange(LoadPlugins());
        }

        public IEnumerable<PluginBase> LoadPlugins()
        {
            string pluginDirectory = "plugins/";
            Directory.CreateDirectory(pluginDirectory);
            foreach (var item in Directory.EnumerateFiles(pluginDirectory, "*.dll"))
            {
                List<PluginBase> plugins = new List<PluginBase>();
                try
                {
                    Assembly assembly = Assembly.Load(Path.GetFullPath(item));
                    var pluginAttributes = assembly.GetCustomAttributes<PluginAssemblyAttribute>();
                    foreach (var plugin in pluginAttributes)
                    {
                         plugins.Add((PluginBase)Activator.CreateInstance(plugin.PluginType));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Could not load plugin assembly", ex);
                }
                foreach (var it in plugins)
                {
                    yield return it;
                }
            }
        }

        internal TriggerPlugin TriggerPlugin { get; private set; }
    }
}
