using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Connection;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    /// <summary>
    /// A event data class for checking user accounts
    /// </summary>
    public class CheckAccountEventArgs : PluginResultEventArgs<bool?>
    {
        /// <summary>
        /// The hash of the current validation process
        /// Same as the 'serverId' mentioned at http://wiki.vg/Authentication#Server_operation
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Instantiate a new instance of the CheckUserAccountEventArgs class with the specific parameters
        /// </summary>
        /// <param name="startResult">The starting value of the Result property. true means that the user is allowed to join,
        /// false means that the user is not allowed to join, null means that it is not sure if the client is allowed to join</param>
        /// <param name="hash">The 'serverId' hash <seealso cref="Hash"/></param>
        /// <param name="connection">The connection object which should be validated</param>
        public CheckAccountEventArgs(bool? startResult, string hash, IProxyConnection connection)
            :base(startResult, connection)
        {
            Hash = hash;
        }
    }
}
