using ProtoBuf;
using STIKS.Server.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace STIKS.Server.Game
{
    public class SceneInfo
    {
        public ConcurrentDictionary<string, SceneItem> StaticItem = new ConcurrentDictionary<string, SceneItem>();

        public ConcurrentDictionary<string, SceneItem> PlayerItem = new ConcurrentDictionary<string, SceneItem>();

        public ConcurrentDictionary<string, SceneItem> EnemyItem = new ConcurrentDictionary<string, SceneItem>();

        public bool Init(ISocketItem item)
        {
            using (var stream = new MemoryStream())
            {
                //stream.WriteByte((byte)((int)CommandList.Init >> 8));
                stream.WriteByte(0);
                stream.WriteByte((byte)CommandList.Init);

                var init = new InitCommandResponse();

                init.StaticItem = StaticItem.Values.ToArray();
                init.PlayerItem = PlayerItem.Values.ToArray();
                init.EnemyItem = EnemyItem.Values.ToArray();

                Serializer.Serialize(stream, init);

                item.Send(stream.ToArray());

                //item.User.SupportArchive = false;
                //item.Send(item.User.SupportArchive ? 
                //    ProtocolTools.ZipCompress(stream.ToArray()) :
                //    stream.ToArray());
            }

            return true;
        }
    }
}
