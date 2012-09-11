using Pdelvo.Minecraft.Proxy.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Service
{
    public partial class ProxyService : ServiceBase
    {
        IProxyServer _server;
        public ProxyService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            _server = new ProxyServer();
            _server.Start();   
        }

        protected override async void OnStop()
        {
            await _server.StopAsync();

            base.OnStop();
        }
    }
}
