using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Connection;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    /// <summary>
    /// A base cancelable event data class with information about a proxy connection
    /// </summary>
    public class UserEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The connection this event data belongs to
        /// </summary>
        public IProxyConnection Connection { get; private set; }

        /// <summary>
        /// Create a new instance of the UserEventArgs class with the specific connection
        /// </summary>
        /// <param name="connection">The connection this event data belongs to</param>
        public UserEventArgs(IProxyConnection connection)
        {
            Connection = connection;
        }
    }
}
