using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Protocol.Packets;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    public static class ConnectionExtensions
    {
        public static void SendServerPacket(this IProxyConnection connection, Packet packet)
        {
            connection.ServerEndPoint.SendPacket(packet);
        }

        public static void SendClientPacket(this IProxyConnection connection, Packet packet)
        {
            connection.ClientEndPoint.SendPacket(packet);
        }
    }
}
