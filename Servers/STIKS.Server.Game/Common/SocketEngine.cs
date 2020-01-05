using Microsoft.AspNetCore.Http;
using STIKS.Common;
using STIKS.Model;
using STIKS.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace STIKS.Server
{
    public class SocketEngine : ISocketItemEvent
    {
        #region Field(s)

        public static SocketEngine Instance { get; } = new SocketEngine();

        public static readonly CancellationTokenSource GlobalCancelToken = new CancellationTokenSource();

        public CancellationTokenSource _CancelToken = new CancellationTokenSource();

        private ConcurrentDictionary<string, ISocketItem> _itemList = new ConcurrentDictionary<string, ISocketItem>();

        private Channel<(ISocketItem item, byte[] msg)> _channelData;

        private Encoding _Encoding = new UTF8Encoding();

        #endregion

        public SocketEngine()
        {
            var options = new BoundedChannelOptions(100000);
            options.FullMode = BoundedChannelFullMode.DropOldest;
            _channelData = Channel.CreateBounded<(ISocketItem item, byte[] msg)>(options);
            
            Task.Run(ProcessThread);
        }

        public async Task<ISocketItem> Add(WebSocket webSocket, string session)
        {
            string key = RedisKeys.GetUserKey(session);

            var userItem = await RedisCacheEngine.Instance.GetObject<UserItem>(key);

            //if (string.IsNullOrEmpty(data))
            //    return null;

            //var userItem = Tools.Deserialize<UserItem>(data);

            //if (userItem == null)
            //    return null;

            if (userItem == null)
                userItem = new UserItem(session, 1);

            userItem.CurrentScene = 1;

            var socketItem = new SocketItem(webSocket, userItem, this);
            _itemList[session] = socketItem;

            return socketItem;
        }

        public void Close(ISocketItem item)
        {
            _itemList.TryRemove(item.UserItem.Session, out var socketItem);
        }

        public bool Receive(ISocketItem item, byte[] msg)
        {
            ProtocolEngine.Instance.Process(item, msg);
            //return _channelData.Writer.TryWrite((item, msg));
            return true;
        }

        private async Task ProcessThread()
        {
            try
            {
                while (!_CancelToken.IsCancellationRequested)
                {
                    var data = await _channelData.Reader.ReadAsync(_CancelToken.Token);

                    if (_CancelToken.IsCancellationRequested)
                        break;

                    var i = _itemList.GetEnumerator();
                    while (i.MoveNext())
                    {
                        if (i.Current.Key == data.item.UserItem.Session)
                            continue;

                        string txt = _Encoding.GetString(data.msg);

                        i.Current.Value.Send($"{data.item.UserItem.Session} -> {txt}");
                    }

                    if (_CancelToken.IsCancellationRequested)
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Save(e);
            }

            if (!_CancelToken.IsCancellationRequested)
                _CancelToken.Cancel();
        }
    }
}
