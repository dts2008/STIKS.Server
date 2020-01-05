using STIKS.Server.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace STIKS.Server
{
    public class CommandAttribute : Attribute
    {
        public CommandList Command { set; get; }
        public CommandAttribute(CommandList command)
        {
            Command = command;
        }
    }
}
