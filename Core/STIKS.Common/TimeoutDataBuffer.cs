using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STIKS.Common
{
    public interface ITimeoutDataBuffer
    {
        void Flush();
    }

    public class TimeoutDataScheduler
    {
        #region Field(s)

        private static int flushTimeout = 10 * 60 * 1000;

        private static CancellationTokenSource _exitToken = new CancellationTokenSource();

        private static ConcurrentBag<ITimeoutDataBuffer> timeoutDataBuffers = new ConcurrentBag<ITimeoutDataBuffer>();

        #endregion

        #region Public Method

        public static void Add(ITimeoutDataBuffer item)
        {
            timeoutDataBuffers.Add(item);
        }

        public static void Start()
        {
            Task.Run(() => ProcessThread());
        }

        public static void Stop()
        {
            _exitToken.Cancel();
        }

        #endregion

        #region Private 

        private static void ProcessThread()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        _exitToken.Token.WaitHandle.WaitOne(flushTimeout);

                        if (_exitToken.IsCancellationRequested)
                            break;

                        foreach (var i in timeoutDataBuffers)
                            i.Flush();

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                    catch (Exception exc)
                    {
                        Logger.Instance.Save(exc, MessageType.Error);
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Instance.Save(exc, MessageType.Error);
            }
        }

        #endregion
    }

    public class TimeoutDataBuffer<TKey, TObject> : ITimeoutDataBuffer
    {
        #region Type(s)
        public class TimeoutDataBufferItem
        {
            /// <summary>
            /// Last update time
            /// </summary>
            public DateTime ObjectTime { get; set; }

            public TObject DataItem { get; set; }

            public TimeoutDataBufferItem(DateTime dateTime, TObject item)
            {
                ObjectTime = dateTime;
                DataItem = item;
            }

            public TimeoutDataBufferItem()
            {
            }
        }

        #endregion

        #region Field(s)

        /// <summary>
        /// Validation timeout
        /// </summary>
        public long Timeout { get; set; } = 3000000000; // 5 * 60 * 10000000

        private volatile ConcurrentDictionary<TKey, TimeoutDataBufferItem> _dataBuffer = new ConcurrentDictionary<TKey, TimeoutDataBufferItem>();

        private static System.Timers.Timer _scheduler;

        private static ConcurrentBag<TimeoutDataBuffer<TKey, TObject>> _schedulerList = new ConcurrentBag<TimeoutDataBuffer<TKey, TObject>>();

        private const long _schedulerMin = 500;

        private const long _schedulerMax = 10000;

        #endregion

        public void Flush()
        {
            try
            {
                //if (_dataBuffer.Count < 500) break;

                var count = 0;
                foreach (var item in _dataBuffer)
                {
                    if (((DateTime.UtcNow.Ticks - item.Value.ObjectTime.Ticks) > Timeout))
                    {
                        _dataBuffer.TryRemove(item.Key, out var l);
                        count++;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Instance.Save(exc, MessageType.Error);
            }
        }

        public TimeoutDataBuffer()
        {
            TimeoutDataScheduler.Add(this);
            //_scheduler = new System.Timers.Timer(15 * 60 * 1000); // 15 minutes                      

            //_scheduler.Elapsed += (source, o) =>
            //{
            //    try
            //    {
            //        foreach (var list in _schedulerList)
            //        {
            //            if (list._dataBuffer.Count < 500) break;

            //            var count = 0;
            //            foreach (var item in list._dataBuffer)
            //            {
            //                if (((DateTime.UtcNow.Ticks - item.Value.ObjectTime.Ticks) > list.Timeout))
            //                {
            //                    list._dataBuffer.TryRemove(item.Key, out var l);
            //                    count++;
            //                }
            //                if (count > _schedulerMax)
            //                    break;
            //            }
            //        }
            //    }
            //    catch (Exception exc)
            //    {
            //        Logger.Instance.Save(exc, MessageType.Error);
            //    }
            //};
            //_scheduler.Start();
        }

        //public TimeoutDataBuffer()
        //{

        //}

        /// <summary>
        /// Counstructor with timeout
        /// </summary>
        /// <param name="timeout">timeout in ticks</param>
        public TimeoutDataBuffer(long timeout) : base()
        {
            Timeout = timeout;
        }

        public bool Get(TKey id, out TObject data)
        {
            data = default(TObject);

            if (_dataBuffer.TryGetValue(id, out TimeoutDataBufferItem item) && (DateTime.UtcNow.Ticks - item.ObjectTime.Ticks) < Timeout)
            {
                data = item.DataItem;
                return true;
            }

            return false;
        }

        public bool GetRenew(TKey id, out TObject data)
        {
            data = default(TObject);

            if (_dataBuffer.TryGetValue(id, out TimeoutDataBufferItem item) && (DateTime.UtcNow.Ticks - item.ObjectTime.Ticks) < Timeout)
            {
                data = item.DataItem;
                item.ObjectTime = DateTime.UtcNow;
                return true;
            }

            return false;
        }

        public bool Renew(TKey id)
        {
            if (_dataBuffer.TryGetValue(id, out TimeoutDataBufferItem item) && (DateTime.UtcNow.Ticks - item.ObjectTime.Ticks) < Timeout)
            {
                item.ObjectTime = DateTime.UtcNow;
                return true;
            }

            return false;
        }

        public void Set(TKey id, TObject item)
        {
            _dataBuffer[id] = new TimeoutDataBufferItem(DateTime.UtcNow, item);
        }

        public void Remove(TKey id)
        {
            _dataBuffer.TryRemove(id, out TimeoutDataBufferItem item);
        }


    }
}
