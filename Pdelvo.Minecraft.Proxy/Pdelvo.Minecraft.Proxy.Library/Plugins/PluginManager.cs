using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    /// <summary>
    /// The main interface between the proxy server and the loaded plugins
    /// </summary>
    public class PluginManager
    {
        List<PluginBase> _plugins = new List<PluginBase>();
        List<IPacketListener> _packetListener = new List<IPacketListener>();

        ILog _logger;

        /// <summary>
        /// The proxy server this plugin manager belongs to
        /// </summary>
        public IProxyServer Server { get; private set; }

        static PluginManager()
        {
            //AppDomain.CurrentDomain.AssemblyResolve += handler;
        }

        /// <summary>
        /// Creates a new instance of the plugin manager class
        /// </summary>
        /// <param name="server">the proxy server this plugin manager belongs to</param>
        public PluginManager(IProxyServer server)
        {
            Server = server;
            _logger = LogManager.GetLogger("Plugin Manager");
            TriggerPlugin = new TriggerPlugin(_plugins);

            _plugins.AddRange(LoadPlugins());
        }

        /// <summary>
        /// Registers a new packet listener. The packet listener will be called when a packet should be redirected from one end point to the other
        /// </summary>
        /// <param name="listener">The packet listener which should be called on incoming packets</param>
        public void RegisterPacketListener(IPacketListener listener)
        {
            _packetListener.Add(listener);
        }

        /// <summary>
        /// Passed a packet sent by a client to the packet listeners
        /// </summary>
        /// <param name="args">A PacketReceivedEventArgs object containing information about the packet</param>
        public void ApplyClientPacket(PacketReceivedEventArgs args)
        {
            List<IPacketListener> removeListener = new List<IPacketListener>();
            foreach (var item in _packetListener)
            {
                bool handled = args.Handled;
                try
                {
                    item.ClientPacketReceived(args);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    args.Handled = handled;//reset value if plugin failed
                    _logger.Error(string.Format("Could not pass client packet {0} to {1}, removing packet listener.", args.Packet, item), ex);
                    removeListener.Add(item);
                }
            }
            _packetListener.RemoveAll(a => removeListener.Contains(a));
        }

        /// <summary>
        /// Passed a packet sent by a server to the packet listeners
        /// </summary>
        /// <param name="args">A PacketReceivedEventArgs object containing information about the packet</param>
        public void ApplyServerPacket(PacketReceivedEventArgs args)
        {
            List<IPacketListener> removeListener = new List<IPacketListener>();
            foreach (var item in _packetListener)
            {
                bool handled = args.Handled;
                try
                {
                    item.ServerPacketReceived(args);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    args.Handled = handled;//reset value if plugin failed
                    _logger.Error(string.Format("Could not pass server packet {0} to {1}, removing packet listener.", args.Packet, item), ex);
                    removeListener.Add(item);
                }
            }
            _packetListener.RemoveAll(a => removeListener.Contains(a));
        }

        private IEnumerable<PluginBase> LoadPlugins()
        {
            string pluginDirectory = "plugins/";
            Directory.CreateDirectory(pluginDirectory);
            foreach (var item in Directory.EnumerateFiles(pluginDirectory, "*.dll"))
            {
                List<PluginBase> plugins = new List<PluginBase>();
                try
                {
                    Assembly assembly = Assembly.Load(File.ReadAllBytes(item));
                    var pluginAttributes = assembly.GetCustomAttributes<PluginAssemblyAttribute>();
                    foreach (var plugin in pluginAttributes)
                    {
                        var pl = (PluginBase)Activator.CreateInstance(plugin.PluginType);
                        plugins.Add(pl);
                        pl.Load(this);
                        _logger.InfoFormat("Loaded plugin {0} in {1}", pl.Name, assembly.GetName().Name);
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
