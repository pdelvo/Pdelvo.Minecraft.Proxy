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

        public override void AllowJoining(CheckIPEventArgs args)
        {
            foreach (var item in _triggerPlugins)
            {
                try
                {
                    item.AllowJoining(args);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Could not pass event 'AllowJoining' to " + item.Name, ex);
                }
            }
        }

        public override void OnPlayerConnected(UserEventArgs args)
        {
            foreach (var item in _triggerPlugins)
            {
                try
                {
                    item.OnPlayerConnected(args);
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

        public override async Task OnUserAccountCheckAsync(CheckAccountEventArgs args)
        {
            foreach (var item in _triggerPlugins)
            {
                try
                {
                    await item.OnUserAccountCheckAsync(args);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Could not pass event 'OnUserAccountCheckAsync' to " + item.Name, ex);
                }
            }
        }

        public override void IsOnlineModeEnabled(PluginResultEventArgs<bool?> args)
        {
            foreach (var item in _triggerPlugins)
            {
                try
                {
                    item.IsOnlineModeEnabled(args);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Could not pass event 'IsOnlineModeEnabled' to " + item.Name, ex);
                }
            }
        }

        public override void GetServerVersion(PluginResultEventArgs<RemoteServerInfo> args)
        {
            foreach (var item in _triggerPlugins)
            {
                try
                {
                    item.GetServerVersion(args);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Could not pass event 'GetServerVersion' to " + item.Name, ex);
                }
            }
        }

        public override void OnUserConnectedCompleted(UserEventArgs args)
        {
            foreach (var item in _triggerPlugins)
            {
                try
                {
                    item.OnUserConnectedCompleted(args);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Could not pass event 'OnUserConnectedCompleted' to " + item.Name, ex);
                }
            }
        }
    }
}
