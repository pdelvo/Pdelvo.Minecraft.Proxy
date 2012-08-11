﻿using System;
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
    public class PluginManager
    {
        List<PluginBase> _plugins = new List<PluginBase>();
        List<IPacketListener> _packetListener = new List<IPacketListener>();

        ILog _logger;

        public IProxyServer Server { get; private set; }

        static PluginManager()
        {
            ResolveEventHandler handler = (s, e) =>
            {
                return Assembly.LoadFile(e.Name);
            };
            AppDomain.CurrentDomain.AssemblyResolve += handler;
        }

        public PluginManager(IProxyServer server)
        {
            Server = server;
            _logger = LogManager.GetLogger("Plugin Manager");
            TriggerPlugin = new TriggerPlugin(_plugins);

            _plugins.AddRange(LoadPlugins());
        }

        public void RegisterPacketListener(IPacketListener listener)
        {
            _packetListener.Add(listener);
        }

        public void ApplyClientPacket(PacketReceivedEventArgs args)
        {
            List<IPacketListener> removeListener = new List<IPacketListener>();
            foreach (var item in _packetListener)
            {
                try
                {
                    item.ClientPacketReceived(args);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Could not pass client packet {0} to {1}, removing packet listener.", args.Packet, item), ex);
                    removeListener.Add(item);
                }
            }
            _packetListener.RemoveAll(a => removeListener.Contains(a));
        }

        public void ApplyServerPacket(PacketReceivedEventArgs args)
        {
            List<IPacketListener> removeListener = new List<IPacketListener>();
            foreach (var item in _packetListener)
            {
                try
                {
                    item.ServerPacketReceived(args);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Could not pass server packet {0} to {1}, removing packet listener.", args.Packet, item), ex);
                    removeListener.Add(item);
                }
            }
            _packetListener.RemoveAll(a => removeListener.Contains(a));
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
                        var pl = (PluginBase)Activator.CreateInstance(plugin.PluginType);
                         plugins.Add(pl);
                         pl.Load(this);
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
