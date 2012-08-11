using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library.Plugins.Events
{
    public class CancelEventArgs : EventArgs
    {
        public bool Canceled { get; protected set; }
        public string CancelMessage { get; protected set; }

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
