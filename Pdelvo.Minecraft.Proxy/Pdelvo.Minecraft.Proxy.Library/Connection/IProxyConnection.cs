using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library.Connection
{
    public interface IProxyConnection : IDisposable
    {
        IProxyEndPoint ServerEndPoint { get; }
        IProxyEndPoint ClientEndPoint { get; }
        Task CloseAsync();
    }
}
