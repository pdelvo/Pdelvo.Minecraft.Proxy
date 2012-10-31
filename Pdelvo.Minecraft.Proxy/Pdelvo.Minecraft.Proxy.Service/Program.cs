using System.ServiceProcess;

namespace Pdelvo.Minecraft.Proxy.Service
{
    internal static class Program
    {
        /// <summary>
        ///   The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
                                {
                                    new ProxyService ()
                                };
            ServiceBase.Run(ServicesToRun);
        }
    }
}