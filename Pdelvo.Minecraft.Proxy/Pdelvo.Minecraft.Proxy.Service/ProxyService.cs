using System.ServiceProcess;
using Pdelvo.Minecraft.Proxy.Library;
using log4net.Config;

namespace Pdelvo.Minecraft.Proxy.Service
{
    public partial class ProxyService : ServiceBase
    {
        private IProxyServer _server;

        public ProxyService()
        {
            InitializeComponent ();
        }

        protected override void OnStart(string[] args)
        {
            XmlConfigurator.Configure ();
            _server = new ProxyServer ();
            _server.Start ();
        }

        protected override async void OnStop()
        {
            await _server.StopAsync ();

            base.OnStop ();
        }
    }
}