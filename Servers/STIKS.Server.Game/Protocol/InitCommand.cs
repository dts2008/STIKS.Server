using STIKS.Server.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace STIKS.Server.Game
{
    [Command(CommandList.Init)]
    public class InitCommand : ICommand
    {
        public bool Process(ISocketItem item, MemoryStream stream)
        {
            SceneEngine.Instance.Process(item);
            return true;
        }
    }
}
