using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Connection;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins
{
    /// <summary>
    /// A base class each proxy server plugin must inherit from
    /// </summary>
    public abstract class PluginBase
    {
        /// <summary>
        /// The name of the plugin
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The version of the plugin
        /// </summary>
        public abstract Version Version { get; }

        /// <summary>
        /// The name of the author of the name
        /// </summary>
        public abstract string Author { get; }


        /// <summary>
        /// This method is being called at the beginning when a new client connects to the proxy server. A plugin can decide to allow, or deny the incoming connection.
        /// </summary>
        /// <param name="address">The IP address of the remote client</param>
        /// <returns>
        /// true, if the client is allowed to join, false if not.
        /// The plugin should return null if the plugin can not decide whether the client is allowed or is not allowed to join.
        /// </returns>
        public virtual bool? AllowJoining(IPAddress address)
        {
            return true;
        }

        /// <summary>
        /// This message is being called once after instantiating the plugin. It is used to initialize the plugin, 
        /// register packet listeners, or storing references to the plugin manager, or other relevant parts of the proxy server
        /// </summary>
        /// <param name="manager">The plugin manager which loaded the plugin</param>
        public abstract void Load(PluginManager manager);

        /// <summary>
        /// This method is called after the proxy verified, that the incoming connection is a user who wants to join the server and the proxy server verified that it knows
        /// the protocol version the client wants to connect with
        /// </summary>
        /// <param name="args">A UserEventArgs which give access to relevant operations like reading user information, or methods to cancel the request</param>
        public virtual void OnPlayerConnected(UserEventArgs args) { }

        /// <summary>
        /// This method is called when the proxy server needs to know to which backend server it should connect to. A plugin can change it, specify the minecraft version,
        /// or cancel it to kick the user.
        /// </summary>
        /// <param name="args">A UserEventArgs which give access to relevant operations like reading user information, and the current server which is a connect candidate,
        /// or methods to cancel the request</param>
        public virtual void OnPlayerServerSelection(PluginResultEventArgs<RemoteServerInfo> args) { }

        /// <summary>
        /// This method is called when the connection to a user is terminated because of the client disconnected, or a network problem appeared
        /// </summary>
        /// <param name="args">The arguments give information about the user, who disconnected from the server</param>
        public virtual void OnConnectionLost(UserEventArgs args) { }

        /// <summary>
        /// In this method a plugin can specify, if a user account is allowed to join with this specific server hash. 
        /// if no plugin specify it the proxy server will contact minecraft.net to check user accounts.
        /// </summary>
        /// <param name="args">Contains the user connection object and the user hash. A plugin can kick users by setting the call canceled</param>
        /// <returns></returns>
        public virtual Task OnUserAccountCheckAsync(CheckAccountEventArgs args) { return Task.FromResult(0); }

        /// <summary>
        /// In this method a plugin can specify if the server should respond to be in online-mode or offline-mode. 
        /// This can be used to allow administrators to join if minecraft.net is down
        /// </summary>
        /// <param name="args">
        /// The argument contains information about the user and the result of this call. true means authentification enabled, 
        /// false means authentification disabled. if the result is null the option in the server configuration file is used
        /// </param>
        public virtual void IsOnlineModeEnabled(PluginResultEventArgs<bool?> args) { }

        /// <summary>
        /// IN this method plugins can specify the protocol version of the server the proxy should connect to
        /// </summary>
        /// <param name="args">Gives information about server and client</param>
        public virtual void GetServerVersion(PluginResultEventArgs<RemoteServerInfo> args) { }
    }
}
