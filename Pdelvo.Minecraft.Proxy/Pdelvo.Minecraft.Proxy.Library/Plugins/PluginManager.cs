using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    public class PluginManager
    {
        List<PluginBase> _plugins = new List<PluginBase>();
        public PluginManager()
        {
        }

        public void LoadPlugins()
        {
            string pluginDirectory = "plugins/";
            Directory.CreateDirectory(pluginDirectory);
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                return Assembly.LoadFile(e.Name);
            };
            foreach (var item in Directory.EnumerateFiles(pluginDirectory, "*.dll"))
            {
                try
                {
                    Assembly assembly = Assembly.Load(Path.GetFullPath(item));
                    Console.WriteLine(item);
                    var plugins = assembly.GetCustomAttributes<PluginAssemblyAttribute>();
                    var plugins2 = assembly.CustomAttributes.Where(a => a.AttributeType == typeof(PluginAssemblyAttribute));
                    foreach (var plugin in plugins)
                    {
                        var instance = Activator.CreateInstance(plugin.PluginType);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
