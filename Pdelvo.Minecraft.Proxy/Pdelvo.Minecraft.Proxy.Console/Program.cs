using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library;
using Pdelvo.Minecraft.Proxy.Library.Plugins;
using log4net;

namespace Pdelvo.Minecraft.Proxy.Console
{
    class Program
    {
        static ILog _logger = LogManager.GetLogger("Program logger");
        static void Main(string[] args)
        {
            System.Console.WriteLine("Press q to quit");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            log4net.Config.XmlConfigurator.Configure();
            IProxyServer server = new ProxyServer();
            server.Start();

            while (System.Console.ReadKey(true).Key != ConsoleKey.Q) { }
            server.StopAsync();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal("Fatal exception occured.", (Exception)e.ExceptionObject);
        }
    }
}
