using System.Net;

namespace Pdelvo.Minecraft.Proxy.Library
{
    /// <summary>
    ///   This class contains information about a backend server
    /// </summary>
    public class RemoteServerInfo
    {
        /// <summary>
        ///   Create a new instance of the RemoteServerInfo class
        /// </summary>
        /// <param name="name"> The name of the backend server </param>
        /// <param name="endPoint"> The ip end point of the backend server </param>
        /// <param name="minecraftVersion"> The minecraft version of the backend server, null for auto detection </param>
        public RemoteServerInfo(string name, IPEndPoint endPoint, int? minecraftVersion)
        {
            Name = name;
            EndPoint = endPoint;
            MinecraftVersion = minecraftVersion;
        }

        /// <summary>
        ///   The name of the backend server
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///   The ip end point it can be accessed from
        /// </summary>
        public IPEndPoint EndPoint { get; set; }

        /// <summary>
        ///   The minecraft version the end point uses, null for auto detection
        /// </summary>
        public int? MinecraftVersion { get; set; }
    }
}