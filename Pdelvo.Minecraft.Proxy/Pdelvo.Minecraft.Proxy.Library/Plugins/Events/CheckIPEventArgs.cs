using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    /// <summary>
    /// Provides information about a given ip address and let plugins decide if the present address is alowed to join
    /// </summary>
    public class CheckIPEventArgs : EventArgs
    {
        /// <summary>
        /// The ip address of the remote end point.
        /// </summary>
        public IPAddress RemoteAddress { get; set; }

        /// <summary>
        /// True if the user is allowed to join, otherwise false.
        /// </summary>
        public bool AllowJoining { get; set; }

        /// <summary>
        /// Creates a new instance of the CheckIPEventArgs class.
        /// </summary>
        /// <param name="remoteAddress">The ip address of the remote end point.</param>
        /// <param name="allowJoining">True if the user is allowed to join, otherwise false.</param>
        public CheckIPEventArgs(IPAddress remoteAddress, bool allowJoining)
        {
            RemoteAddress = remoteAddress;
            AllowJoining = allowJoining;
        }
    }
}
