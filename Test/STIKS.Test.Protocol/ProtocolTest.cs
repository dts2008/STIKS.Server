using STIKS.Server.Protocol;
using System;
using Xunit;
using System.IO;
using ProtoBuf;
using System.Collections.Generic;
using NSubstitute;

namespace STIKS.Test.Protocol
{
    public class ProtocolTest
    {
        [Fact]
        public void ResponceInit()
        {
            int countSceneItems = 10;
            var item = new InitCommandResponse();

            var list = new List<SceneItem>();

            for (int i = 0; i < countSceneItems; ++i)
            {
                var sceneItem = new SceneItem();

                sceneItem.Id = i + 1;
                sceneItem.ItemType = SceneItemType.Player;
                sceneItem.Position = new VectorItem(i + 1, i + 1, i + 1);
                sceneItem.Move = new VectorItem(i + 100, i + 100, i + 100);
                sceneItem.Tag = Guid.NewGuid().ToString("N");

                list.Add(sceneItem);
            }

            item.StaticItem = list.ToArray();
            item.PlayerItem = list.ToArray();
            item.EnemyItem = list.ToArray();

            byte[] buff;
            using (var memory = new MemoryStream())
            {
                Serializer.Serialize(memory, item);

                buff = memory.ToArray();
            }

            InitCommandResponse restore = null;
            using (var memory = new MemoryStream(buff))
            {
                restore = Serializer.Deserialize<InitCommandResponse>(memory);
            }

            //var calculator = Substitute.
            Assert.NotNull(restore);
            Assert.NotNull(restore.StaticItem);
            Assert.NotNull(restore.PlayerItem);
            Assert.NotNull(restore.EnemyItem);
            Assert.Equal(restore.StaticItem.Length, countSceneItems);
        }
    }
}
