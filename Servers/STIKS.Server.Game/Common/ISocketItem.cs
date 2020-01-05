using STIKS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STIKS.Server
{
    public interface ISocketItem
    {
        Task Receive();

        void Send(byte[] msg);

        void Send(string msg);

        void Close();

        UserItem UserItem { get; }
    }
}
