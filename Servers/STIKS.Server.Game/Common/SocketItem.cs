using STIKS.Common;
using STIKS.Model;
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
    public class SocketItem : ISocketItem
    {
        #region Const(s)

        public int ReceiveBufferSize = 1024 * 8;

        public int MaxMessage = 1000;

        #endregion Const(s)

        #region Field(s)

        private WebSocket _Client;

        private Channel<byte[]> _ChannelData;

        private int _CountData;

        private Encoding _Encoding = new UTF8Encoding();

        public CancellationTokenSource _CancelToken = new CancellationTokenSource();

        public UserItem UserItem { get; private set; }

        private ISocketItemEvent _ItemEvent = null;

        #endregion Field(s)

        #region Constructor

        public SocketItem(WebSocket client, UserItem userItem, ISocketItemEvent itemEvent)
        {
            _Client = client;
            UserItem = userItem;
            _ItemEvent = itemEvent;

            var options = new BoundedChannelOptions(MaxMessage);
            options.FullMode = BoundedChannelFullMode.DropOldest;

            _ChannelData = Channel.CreateBounded<byte[]>(options);
        }

        public async Task Receive()
        {
            _ = Send();
            //_ = Task.Run(Send);

            var buffer = new byte[ReceiveBufferSize];

            try
            {
                WebSocketReceiveResult result = await _Client.ReceiveAsync(new ArraySegment<byte>(buffer), _CancelToken.Token);

                var isCancelled = result.MessageType == WebSocketMessageType.Close || result.CloseStatus.HasValue || _CancelToken.IsCancellationRequested;

                while (!isCancelled)
                {
                    int count = 0;

                    do
                    {
                        result = await _Client.ReceiveAsync(new ArraySegment<byte>(buffer, count, ReceiveBufferSize - count), _CancelToken.Token);

                        isCancelled = result.MessageType == WebSocketMessageType.Close || result.CloseStatus.HasValue || _CancelToken.IsCancellationRequested;

                        if (!isCancelled) count += result.Count;
                    }
                    while (!result.EndOfMessage && !isCancelled);

                    if (!isCancelled)
                    {
                        if (!Process(buffer, count))
                            break;
                        // Process Data
                    }
                }

                await _Client.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, SocketEngine.GlobalCancelToken.Token);
            }
            catch (Exception e)
            {
                Logger.Instance.Save(e);
            }

            Clear();
            _ItemEvent.Close(this);
        }

        public void Send(string msg)
        {
            Send(_Encoding.GetBytes(msg));
        }

        public void Send(byte[] msg)
        {
            if (_CountData > MaxMessage)
            {
                Clear();
                _ItemEvent.Close(this);

                return;
            }
            _CountData++;

            _ChannelData.Writer.TryWrite(msg);
        }

        public void Close()
        {
            Clear();
        }

        #endregion

        #region Private method

        private async Task Send()
        {
            try
            {
                while (!_CancelToken.IsCancellationRequested)
                {
                    byte[] msg = await _ChannelData.Reader.ReadAsync(_CancelToken.Token);

                    if (_CancelToken.IsCancellationRequested)
                        break;

                    await _Client.SendAsync(new ArraySegment<byte>(msg), WebSocketMessageType.Binary, true, _CancelToken.Token);

                    _CountData--;

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

        private bool Process(byte[] msg, int count)
        {
            var data = new byte[count];
            Buffer.BlockCopy(msg, 0, data, 0, count);
            //_Encoding.GetString(buffer, 0, count)
            if (!_ItemEvent.Receive(this, data))
            {
                // close ?
            }

            return true;
        }

        private void Clear()
        {
            if (!_CancelToken.IsCancellationRequested)
                _CancelToken.Cancel();

            _ChannelData.Writer.Complete();
        }

        #endregion
    }
}
