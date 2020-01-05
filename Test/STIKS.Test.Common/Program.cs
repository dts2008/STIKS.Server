using ProtoBuf;
using STIKS.Common;
using STIKS.Model;
using STIKS.Redis;
using STIKS.Server.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace STIKS.Test.Common
{
    class Program
    {
        static void Main(string[] args)
        {
            //RedisTest();
            //ResponceInit();

            //var calculator = Substitute.
            //Assert.NotNull(restore);
            //Assert.NotNull(restore.SceneItems);
            //Assert.Equal(restore.SceneItems.Length, countSceneItems);

            Console.WriteLine("Hello World!");
        }

        public static void UserProtobuf()
        {
            var item = new UserItem(Guid.NewGuid().ToString("N"), 1);
            item.CurrentScene = 2;

            //item.ToByteArray();
        }

        public static void RedisTest()
        {
            RedisCacheEngine.Instance.Connect();

            var sw = new Stopwatch();

            sw.Start();
            for (int i = 0; i < 10000; ++i)
            {
                var item = new UserItem(Guid.NewGuid().ToString("N"), 1);
                item.CurrentScene = 2;

                var result = RedisCacheEngine.Instance.SetObject<UserItem>($"stiks:session.{item.Session}", item, new TimeSpan(0, 0, 300)).Result;

                var getItem = RedisCacheEngine.Instance.GetObject<UserItem>($"stiks:session.{item.Session}").Result;
            }

            sw.Stop();
            Console.WriteLine($"Takes for hash set: {sw.ElapsedMilliseconds}");

            sw.Start();
            for (int i = 0; i < 10000; ++i)
            {
                var item = new UserItem(Guid.NewGuid().ToString("N"), 1);
                item.CurrentScene = 2;

                var result = RedisCacheEngine.Instance.Set($"stiks:session.{item.Session}", Tools.ToJson(item), new TimeSpan(0, 0, 300)).Result;

                var getItem = Tools.Deserialize<UserItem>(RedisCacheEngine.Instance.Get($"stiks:session.{item.Session}").Result);
            }

            sw.Stop();
            Console.WriteLine($"Takes for json set: {sw.ElapsedMilliseconds}");

        }

        public static void ResponceInit()
        {
            int countSceneItems = 20;
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

            //ZipCompress()

            var zipped = ZipCompress(buff);

            var decompress = ZipDecompress(zipped);

            InitCommandResponse restore = null;
            using (var memory = new MemoryStream(decompress))
            {
                restore = Serializer.Deserialize<InitCommandResponse>(memory);
            }

            //var calculator = Substitute.
            //Assert.NotNull(restore);
            //Assert.NotNull(restore.SceneItems);
            //Assert.Equal(restore.SceneItems.Length, countSceneItems);
        }

        public static byte[] ZipCompress(string data)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = zipArchive.CreateEntry("zip");

                    using (var entryStream = demoFile.Open())
                    {
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            streamWriter.Write(data);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public static byte[] ZipCompress(byte[] data)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = zipArchive.CreateEntry("zip");

                    using (var entryStream = demoFile.Open())
                    {
                        entryStream.Write(data, 0, data.Length);
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public static byte[] ZipDecompress(byte[] data)
        {
            try
            {
                using (var zippedStream = new MemoryStream(data))
                {
                    using (var archive = new ZipArchive(zippedStream))
                    {
                        var entry = archive.Entries.FirstOrDefault();

                        if (entry != null)
                        {
                            using (var unzippedEntryStream = entry.Open())
                            {
                                using (var ms = new MemoryStream())
                                {
                                    unzippedEntryStream.CopyTo(ms);

                                    return ms.ToArray();
                                }
                            }
                        }

                        return null;
                    }
                }

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
