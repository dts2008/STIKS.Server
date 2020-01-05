using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace STIKS.Server.Protocol
{
    [ProtoContract]
    public class VectorItem
    {
        [ProtoMember(1)]
        public float X { get; set; }

        [ProtoMember(2)]
        public float Y { get; set; }

        [ProtoMember(3)]
        public float Z { get; set; }

        public VectorItem(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public VectorItem()
        {
        }
    }
}
