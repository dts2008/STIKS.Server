using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace STIKS.Server.Protocol
{
    public enum SceneItemType
    {
        Player = 1,
        Enemy = 2,
        StaticObject = 3,
        Trophy = 4
    }

    [ProtoContract]
    public class SceneItem
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public int Revicion { get; set; } = 1;

        [ProtoMember(3)]
        public string Tag { get; set; }

        [ProtoMember(4)]
        public SceneItemType ItemType { get; set; }

        [ProtoMember(5)]
        public VectorItem Position { get; set; }

        [ProtoMember(6)]
        public VectorItem Move { get; set; }

    }
}
