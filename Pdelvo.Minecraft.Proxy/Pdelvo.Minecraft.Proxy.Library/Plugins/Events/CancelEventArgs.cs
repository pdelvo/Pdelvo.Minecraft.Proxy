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
    }
}
