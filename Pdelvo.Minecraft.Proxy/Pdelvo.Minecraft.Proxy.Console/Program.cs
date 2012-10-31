using System;
using Pdelvo.Minecraft.Proxy.Library;
using log4net;
using log4net.Config;

namespace Pdelvo.Minecraft.Proxy.Console
{
    internal class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger("Program logger");

        private static void Main(string[] args)
        {
            System.Console.WriteLine("Press q to quit");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            XmlConfigurator.Configure ();
            IProxyServer server = new ProxyServer ();
            server.Start ();

            while (System.Console.ReadKey(true).Key != ConsoleKey.Q)
            {
            }
            server.StopAsync ();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal("Fatal exception occured.", (Exception) e.ExceptionObject);
        }
    }
}