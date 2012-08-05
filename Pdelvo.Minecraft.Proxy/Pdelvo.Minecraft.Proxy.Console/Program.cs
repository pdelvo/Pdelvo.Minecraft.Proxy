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
            IProxyServer server = new ProxyServer(new IPEndPoint(IPAddress.Any, 25565));
            server.Start();
            System.Console.ReadKey();
            server.StopAsync();
            System.Console.ReadKey();
        }
    }
}
