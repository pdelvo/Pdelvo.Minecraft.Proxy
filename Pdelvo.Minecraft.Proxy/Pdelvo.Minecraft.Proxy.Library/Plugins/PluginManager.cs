using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;
using log4net;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    /// <summary>
    ///   The main interface between the proxy server and the loaded plugins
    /// </summary>
    public class PluginManager
    {
        private readonly ILog _logger;
        private readonly List<IPacketListener> _packetListener = new List<IPacketListener> ();
        private readonly List<PluginBase> _plugins = new List<PluginBase> ();

        /// <summary>
        ///   Creates a new instance of the plugin manager class
        /// </summary>
        /// <param name="server"> the proxy server this plugin manager belongs to </param>
        public PluginManager(IProxyServer server)
        {
            Server = server;
            _logger = LogManager.GetLogger("Plugin Manager");
            TriggerPlugin = new TriggerPlugin(_plugins);

            _plugins.AddRange(LoadPlugins ());
        }

        /// <summary>
        ///   The proxy server this plugin manager belongs to
        /// </summary>
        public IProxyServer Server { get; private set; }

        internal TriggerPlugin TriggerPlugin { get; private set; }

        /// <summary>
        ///   Registers a new packet listener. The packet listener will be called when a packet should be redirected from one end point to the other
        /// </summary>
        /// <param name="listener"> The packet listener which should be called on incoming packets </param>
        public void RegisterPacketListener(IPacketListener listener)
        {
            _packetListener.Add(listener);
        }

        /// <summary>
        ///   Passed a packet sent by a client to the packet listeners
        /// </summary>
        /// <param name="args"> A PacketReceivedEventArgs object containing information about the packet </param>
        public void ApplyClientPacket(PacketReceivedEventArgs args)
        {
            var removeListener = new List<IPacketListener> ();
            foreach (IPacketListener item in _packetListener)
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
                    args.Handled = handled; //reset value if plugin failed
                    _logger.Error(
                        string.Format("Could not pass client packet {0} to {1}, removing packet listener.", args.Packet,
                                      item), ex);
                    removeListener.Add(item);
                }
            }
            _packetListener.RemoveAll(a => removeListener.Contains(a));
        }

        /// <summary>
        ///   Passed a packet sent by a server to the packet listeners
        /// </summary>
        /// <param name="args"> A PacketReceivedEventArgs object containing information about the packet </param>
        public void ApplyServerPacket(PacketReceivedEventArgs args)
        {
            var removeListener = new List<IPacketListener> ();
            foreach (IPacketListener item in _packetListener)
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
                    args.Handled = handled; //reset value if plugin failed
                    _logger.Error(
                        string.Format("Could not pass server packet {0} to {1}, removing packet listener.", args.Packet,
                                      item), ex);
                    removeListener.Add(item);
                }
            }
            _packetListener.RemoveAll(a => removeListener.Contains(a));
        }

        private IEnumerable<PluginBase> LoadPlugins()
        {
            string pluginDirectory = "plugins/";
            Directory.CreateDirectory(pluginDirectory);
            foreach (string item in Directory.EnumerateFiles(pluginDirectory, "*.dll"))
            {
                var plugins = new List<PluginBase> ();
                try
                {
                    Assembly assembly = Assembly.Load(File.ReadAllBytes(item));
                    IEnumerable<PluginAssemblyAttribute> pluginAttributes =
                        assembly.GetAttributes<PluginAssemblyAttribute> ();
                    foreach (PluginAssemblyAttribute plugin in pluginAttributes)
                    {
                        var pl = (PluginBase) Activator.CreateInstance(plugin.PluginType);
                        plugins.Add(pl);
                        pl.Load(this);
                        _logger.InfoFormat("Loaded plugin {0} in {1}", pl.Name, assembly.GetName ().Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Could not load plugin assembly", ex);
                }
                foreach (PluginBase it in plugins)
                {
                    yield return it;
                }
            }
        }
    }
}