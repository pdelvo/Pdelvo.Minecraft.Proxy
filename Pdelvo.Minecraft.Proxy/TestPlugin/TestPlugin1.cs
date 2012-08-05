using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Plugins;

namespace TestPlugin
{
    public class TestPlugin1 : PluginBase
    {
        public override string Name
        {
            get { return "Test Plugin 1"; }
        }

        public override Version Version
        {
            get { return new Version("0.0.0.1"); }
        }

        public override string Author
        {
            get { return "pdelvo"; }
        }

        public TestPlugin1()
        {
            Console.WriteLine("Hallo, Welt! 2");
        }
    }
}
