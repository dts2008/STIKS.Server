using STIKS.Server.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace STIKS.Server.Game
{
    [Command(CommandList.Move)]
    public class MoveCommand : ICommand
    {
        public bool Process(ISocketItem item, MemoryStream stream)
        {
            //float x = reader.ReadSingle();
            //float y = reader.ReadSingle();
            //float z = reader.ReadSingle();

            return true;
        }
    }
}
