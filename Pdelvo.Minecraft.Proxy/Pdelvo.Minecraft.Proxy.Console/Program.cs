using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library;
using Pdelvo.Minecraft.Proxy.Library.Plugins;

namespace Pdelvo.Minecraft.Proxy.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Press q to quit");
            log4net.Config.XmlConfigurator.Configure();
            IProxyServer server = new ProxyServer();
            server.Start();

            while (System.Console.ReadKey(true).Key != ConsoleKey.Q) { }
            server.StopAsync();
        }
    }
}
