using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    internal class TriggerPlugin : PluginBase
    {
        List<PluginBase> _triggerPlugins;

        ILog _logger;

        public TriggerPlugin()
        {
            _logger = LogManager.GetLogger("Plugins");
        }

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

        public override void OnPlayerConnected(UserEventArgs connection)
        {
            foreach (var item in _triggerPlugins)
            {
                try
                {
                    item.OnPlayerConnected(connection);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Could not pass event 'OnPlayerConnected' to " + item.Name, ex);
                }
            }
        }

        public override void OnPlayerServerSelection(PluginResultEventArgs<RemoteServerInfo> args)
        {
            foreach (var item in _triggerPlugins)
            {
                try
                {
                    item.OnPlayerServerSelection(args);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Could not pass event 'OnPlayerServerSelection' to " + item.Name, ex);
                }
            }
        }

        public override void OnConnectionLost(UserEventArgs args)
        {
            foreach (var item in _triggerPlugins)
            {
                try
                {
                    item.OnConnectionLost(args);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Could not pass event 'OnConnectionLost' to " + item.Name, ex);
                }
            }
        }

        public override void Load(PluginManager manager)
        {
            throw new NotSupportedException();
        }
    }
}
