using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;
using log4net;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    internal class TriggerPlugin : PluginBase
    {
        private readonly ILog _logger;
        private readonly List<PluginBase> _triggerPlugins;

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
            get { throw new NotSupportedException (); }
        }

        public override Version Version
        {
            get { throw new NotSupportedException (); }
        }

        public override string Author
        {
            get { throw new NotSupportedException (); }
        }

        public override void AllowJoining(CheckIPEventArgs args)
        {
            foreach (PluginBase item in _triggerPlugins)
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
            foreach (PluginBase item in _triggerPlugins)
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
            foreach (PluginBase item in _triggerPlugins)
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
            foreach (PluginBase item in _triggerPlugins)
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
            throw new NotSupportedException ();
        }

        public override async Task OnUserAccountCheckAsync(CheckAccountEventArgs args)
        {
            foreach (PluginBase item in _triggerPlugins)
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
            foreach (PluginBase item in _triggerPlugins)
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
            foreach (PluginBase item in _triggerPlugins)
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
            foreach (PluginBase item in _triggerPlugins)
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