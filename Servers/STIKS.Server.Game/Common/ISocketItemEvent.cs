using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STIKS.Server
{
    public interface ISocketItemEvent
    {
        bool Receive(ISocketItem item, byte[] msg);

        void Close(ISocketItem item);
    }
}
