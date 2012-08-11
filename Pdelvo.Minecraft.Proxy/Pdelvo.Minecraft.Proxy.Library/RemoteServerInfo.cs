using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library
{
    public class RemoteServerInfo
    {
        public string Name { get; private set; }
        public IPEndPoint EndPoint { get; private set; }
        public int MinecraftVersion { get; private set; }

        public RemoteServerInfo(string name, IPEndPoint endPoint, int minecraftVersion)
        {
            Name = name;
            EndPoint = endPoint;
            MinecraftVersion = minecraftVersion;
        }
    }
}
