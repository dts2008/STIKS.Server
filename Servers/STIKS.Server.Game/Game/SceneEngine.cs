using STIKS.Server.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STIKS.Server.Game
{
    public class SceneEngine
    {
        public ConcurrentDictionary<int, SceneInfo> Scenes = new ConcurrentDictionary<int, SceneInfo>();

        public static SceneEngine Instance { get; private set; } = new SceneEngine();

        public int defScene = 1;

        public SceneEngine()
        {
            int countSceneItems = 200;
            var scene = new SceneInfo();

            //var list = new List<SceneItem>();

            for (int i = 0; i < countSceneItems; ++i)
            {
                var sceneItem = new SceneItem();

                sceneItem.Id = i + 1;
                sceneItem.ItemType = SceneItemType.Player;
                sceneItem.Position = new VectorItem(i + 1, i + 1, i + 1);
                sceneItem.Move = new VectorItem(i + 100, i + 100, i + 100);
                sceneItem.Tag = Guid.NewGuid().ToString("N");

                scene.StaticItem[Guid.NewGuid().ToString("N")] = sceneItem;
                scene.PlayerItem[Guid.NewGuid().ToString("N")] = sceneItem;
                scene.EnemyItem[Guid.NewGuid().ToString("N")] = sceneItem;
                //list.Add(sceneItem);
            }

            //scene.StaticItem = list.ToArray();
            //scene.PlayerItem = list.ToArray();
            //scene.EnemyItem = list.ToArray();

            Scenes[defScene] = scene;
        }

        public bool Process(ISocketItem item)
        {
            
            Scenes.TryGetValue(item.UserItem.CurrentScene, out var scene);

            // scene
            scene.Init(item);
            return true;
        }
    }
}
