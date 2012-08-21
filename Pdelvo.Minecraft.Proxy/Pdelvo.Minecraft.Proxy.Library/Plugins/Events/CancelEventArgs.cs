using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    /// <summary>
    /// A base class for cancelable EventArgs
    /// </summary>
    public class CancelEventArgs : EventArgs
    {
        /// <summary>
        /// true if this operation is canceled, otherwise false
        /// </summary>
        public bool Canceled { get; protected set; }

        /// <summary>
        /// The message why this operation is canceled. Null if it is not canceled
        /// </summary>
        public string CancelMessage { get; protected set; }

        /// <summary>
        /// Cancel this operation with a specific message
        /// </summary>
        /// <param name="message">The reason why this operation is canceled</param>
        public virtual void SetCanceled(string message)
        {
            Canceled = true;
            CancelMessage = message;
        }

        /// <summary>
        /// This message will throw a OperationCanceledException with the <see cref="CancelMessage">CancelMessage</see> if the <see cref="Canceled">Canceled</see> flag is set to true
        /// </summary>
        public void EnsureSuccess()
        {
            if (Canceled) throw new OperationCanceledException(CancelMessage);
        }
    }
}
