using STIKS.Common;
using STIKS.Server.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace STIKS.Server
{
    public class ProtocolEngine
    {
        #region Field(s)

        private Dictionary<CommandList, ICommand> _Commands = new Dictionary<CommandList, ICommand>();

        public static ProtocolEngine Instance { get; private set; } = new ProtocolEngine();

        #endregion Field(s)

        #region Public method(s)

        public void Init()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(ICommand));

            Type[] types = assembly.GetTypes();
            
            if (types == null || types.Length == 0) 
                return;

            for (int i = 0; i < types.Length; ++i)
            {
                var attribute = types[i].GetCustomAttribute<CommandAttribute>();
                if (attribute == null) continue;

                ICommand command = (ICommand)Activator.CreateInstance(types[i]);// as SlotEngineRNG;

                _Commands[attribute.Command] = command;
            }
        }

        public bool Process(ISocketItem item, byte[] data)
        {
            try
            {
                int commandType = (data[0] << 8) + data[1];

                using (var stream = new MemoryStream(data, 2, data.Length - 2))
                {
                    if (!_Commands.TryGetValue((CommandList)commandType, out var command))
                        return false;

                    return command.Process(item, stream);
                    //using (var reader = new BinaryReader(stream))
                    //{
                    //    int commandType = reader.ReadInt16();

                    //    if (!_Commands.TryGetValue((CommandType)commandType, out var command))
                    //        return false;

                    //    return command.Process(item, reader);
                    //}
                }
            }
            catch (Exception exc)
            {
                Logger.Instance.Save(exc);
            }

            return true;
        }

        #endregion Public method(s)
    }
}
