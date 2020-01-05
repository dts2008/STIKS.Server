using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace STIKS.Server
{
    public interface ICommand
    {
        public bool Process(ISocketItem item, MemoryStream stream);
    }
}
