using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Connection;
using Pdelvo.Minecraft.Proxy.Library.Plugins;

namespace Pdelvo.Minecraft.Proxy.Library
{
    /// <summary>
    ///   A base interface a proxy server must implement.
    /// </summary>
    public interface IProxyServer : IDisposable
    {
        /// <summary>
        ///   Get the current plugin manager.
        /// </summary>
        PluginManager PluginManager { get; }

        /// <summary>
        ///   Get the current local end point.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        ///   Get the current count of connected users.
        /// </summary>
        int ConnectedUsers { get; }

        /// <summary>
        ///   True if the server is in online mode, otherwise false.
        /// </summary>
        bool OnlineMode { get; }

        /// <summary>
        ///   Get the maximum number of users which are allowed to connect to the server.
        /// </summary>
        int MaxUsers { get; set; }

        /// <summary>
        ///   Get or set the message of the day.
        /// </summary>
        string MotD { get; set; }

        /// <summary>
        ///   True if the proxy is currently waiting for new connections, otherwise false.
        /// </summary>
        bool Listening { get; }

        /// <summary>
        ///   Get a list of all open proxy connections.
        /// </summary>
        IEnumerable<IProxyConnection> OpenConnections { get; }

        /// <summary>
        ///   Start listening for new connections.
        /// </summary>
        void Start();

        /// <summary>
        ///   close all connections and stop internal tasks asynchronously.
        /// </summary>
        /// <returns> A task representating the shutdown process. </returns>
        Task StopAsync();

        /// <summary>
        ///   Get a new endpoint for a given IProxyConnection
        /// </summary>
        /// <param name="proxyConnection"> The proxyconnection which need to get a new server end point. </param>
        /// <returns> A RemoteServerInfo object which contains information about the new server end point. </returns>
        RemoteServerInfo GetServerEndPoint(IProxyConnection proxyConnection);
    }
}