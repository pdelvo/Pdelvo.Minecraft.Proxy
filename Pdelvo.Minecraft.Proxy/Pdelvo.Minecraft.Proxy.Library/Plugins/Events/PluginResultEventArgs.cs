using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Proxy.Library.Connection;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    /// <summary>
    /// A base class for event data which should find a generic result of type T
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    public class PluginResultEventArgs<T> : UserEventArgs
    {
        /// <summary>
        /// The result of the current operation
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Creates a new instance of the PluginResultEventArgs class
        /// </summary>
        /// <param name="result">The starting value of the result</param>
        /// <param name="connection">The connection this operation belongs to</param>
        public PluginResultEventArgs(T result, IProxyConnection connection)
            :base(connection)
        {
            Result = result;
        }
    }
}
