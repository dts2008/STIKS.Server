using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace STIKS.Server.Protocol
{
    [ProtoContract]
    public class InitCommandResponse
    {
        [ProtoMember(1)]
        public SceneItem[] StaticItem;

        [ProtoMember(2)]
        public SceneItem[] PlayerItem;

        [ProtoMember(3)]
        public SceneItem[] EnemyItem;

        [ProtoMember(4)]
        public SceneItem[] TrophyItem;

        [ProtoMember(5)]
        public SceneItem[] BulletsItem;

    }
}
