using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using STIKS.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace STIKS.Redis
{
    public class RedisCacheEngine
    {
        #region Field(s) 

        private string _Address = "127.0.0.1";

        private int _Port = 6379;

        private string _Password = string.Empty;

        private int _TryReconnect = 3;

        private int _TryTimeouts = 5;

        private long _ReconnectCounter = 1;

        private int _ConnectTimeout = 5000;

        private int _SyncTimeout = 5000;

        private ConfigurationOptions _ConfigurationOptions;

        private SemaphoreSlim _Locker = new SemaphoreSlim(1, 1);
        //private object _lcObject = new object();

        public static TimeSpan DefExpirationTime = new TimeSpan(15, 0, 0, 0);

        public static string RedisNamespace = "stiks:";

        private string _TestRedisNamespace = RedisNamespace + "test";

        public ConnectionMultiplexer Client { get; private set; } = null;

        public IDatabase Database { get; private set; } = null;

        public bool IsConected = false;

        static public RedisCacheEngine Instance { get; } = new RedisCacheEngine();

        private ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _Properties = new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();

        
        private Dictionary<Type, Func<object, RedisValue>> _GetRedisValue = new Dictionary<Type, Func<object, RedisValue>> {
            { typeof(int), (object o) => { return (int)o; } },
            { typeof(long), (object o) => { return (long)o; } },
            { typeof(float), (object o) => { return (float)o; } },
            { typeof(double), (object o) => { return (double)o; } },
            { typeof(string), (object o) => { return (string)o; } }
        };

        private Dictionary<Type, Action<RedisValue, PropertyInfo, object>> _SetRedisValue = new Dictionary<Type, Action<RedisValue, PropertyInfo, object>> {
            { typeof(int), (RedisValue r, PropertyInfo p, object o) => {  p.SetValue(o, (int)r); } },
            { typeof(long), (RedisValue r, PropertyInfo p, object o) => {  p.SetValue(o, (long)r); } },
            { typeof(float), (RedisValue r, PropertyInfo p, object o) => {  p.SetValue(o, (float)r); } },
            { typeof(double), (RedisValue r, PropertyInfo p, object o) => {  p.SetValue(o, (double)r); } },
            { typeof(string), (RedisValue r, PropertyInfo p, object o) => {  p.SetValue(o, (string)r); } }
        };

        #endregion Field(s) 

        #region Public method

        public bool Connect()
        {

            try
            {
                IsConected = false;
                
                _Address = AppSettings.Instance.GetSection("Redis").GetValue<string>("Address");
                _Port = AppSettings.Instance.GetSection("Redis").GetValue<int>("Port");
                _Password = AppSettings.Instance.GetSection("Redis").GetValue<string>("Password");

                int connectTimeout = AppSettings.Instance.GetSection("Redis").GetValue<int>("ConnectTimeout");
                if (connectTimeout > 0) _ConnectTimeout = connectTimeout;

                int syncTimeout = AppSettings.Instance.GetSection("Redis").GetValue<int>("SyncTimeout");
                if (syncTimeout > 0) _SyncTimeout = syncTimeout;

                string redisNamespace = AppSettings.Instance.GetSection("Redis").GetValue<string>("Namespace");
                if (!string.IsNullOrEmpty(redisNamespace))
                {
                    RedisNamespace = redisNamespace + ":";
                    _TestRedisNamespace = RedisNamespace + "test";
                }


                _ConfigurationOptions = new ConfigurationOptions()
                {
                    EndPoints = { { $"{_Address}:{_Port}" } },
                    ConnectTimeout = _ConnectTimeout,
                    SyncTimeout = _SyncTimeout,
                    AbortOnConnectFail = false
                };

                if (!string.IsNullOrEmpty(_Password)) _ConfigurationOptions.Password = _Password;

                Client = ConnectionMultiplexer.Connect(_ConfigurationOptions);


                var interval = new TimeSpan(0, 0, 1);
                Database = Client.GetDatabase();
                
                IsConected = Database.StringSet(_TestRedisNamespace, _TestRedisNamespace, interval);

                if (IsConected)
                    Logger.Instance.Save($"Connected to Redis {_ConfigurationOptions.EndPoints}");

            }
            catch (Exception exc)
            {
                Logger.Instance.Save(exc);
            }

            return IsConected;
        }

        #endregion

        #region Private method(s)

        public async Task<T> RedisExecFunction<T>(Func<Task<T>> func)
        {
            T result = default(T);

            if (!IsConected || Database == null)
                return result;

            bool isDisconnect = false;
            bool isSentRequest = true;

            int tryTimeouts = 0;
            int tryReconnect = 0;

            while (isSentRequest && tryTimeouts <= _TryTimeouts && tryReconnect <= _TryReconnect)
            {
                tryTimeouts++;
                isSentRequest = false;

                try
                {
                    result = await func();
                }
                catch (RedisConnectionException exc)
                {
                    isDisconnect = true;
                    Logger.Instance.Save(exc);
                }
                catch (RedisTimeoutException exc)
                {
                    isSentRequest = true;
                    Logger.Instance.Save(exc);
                }

                if (isDisconnect)
                {
                    // RECONNECT
                    ConnectionMultiplexer clientReconnect = null;
                    bool isTryConnect = false;

                    long currentCounter = Interlocked.Read(ref _ReconnectCounter);

                    await _Locker.WaitAsync();

                    long lockCounter = Interlocked.Read(ref _ReconnectCounter);

                    if (lockCounter == currentCounter)
                    {
                        while (tryReconnect <= _TryReconnect)
                        {
                            ++tryReconnect;

                            bool needContinue = false;
                            try
                            {
                                clientReconnect = await ConnectionMultiplexer.ConnectAsync(_ConfigurationOptions);
                                //clientReconnect = ConnectionMultiplexer.Connect(_ConfigurationOptions);

                                if (await Client.GetDatabase().StringSetAsync(_TestRedisNamespace, _TestRedisNamespace, new TimeSpan(0, 0, 1)))
                                {
                                    isSentRequest = true;
                                    Client = clientReconnect;
                                    Database = Client.GetDatabase();
                                }
                            }
                            catch (RedisConnectionException)
                            {
                                needContinue = true;
                            }
                            catch (RedisTimeoutException)
                            {
                                needContinue = true;
                            }
                            catch (Exception)
                            {
                            }

                            if (!needContinue) break;
                        }

                        Interlocked.Increment(ref _ReconnectCounter);
                    }
                    else if (Client.IsConnected) isTryConnect = true;

                    _Locker.Release();

                    if (isTryConnect) isSentRequest = true;
                }
            }

            return result;
        }

        public Dictionary<string, PropertyInfo> GetProperties(Type type)
        {
            if (!_Properties.TryGetValue(type, out var properties))
            {
                properties = new Dictionary<string, PropertyInfo>();
                
                foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    properties[p.Name] = p;
                }

                _Properties[type] = properties;
            }

            return properties;
        }

        #endregion Private method(s)

        #region Public method(s)

        public async Task<bool> Set(string key, RedisValue value, TimeSpan interval, When when = When.Always)
        {
            return await RedisExecFunction<bool>(async () =>
                await Database.StringSetAsync(RedisNamespace + key, value, interval, when)
            );
        }

        public async Task<string> Get(string key)
        {
            return await RedisExecFunction<string>(async() =>
                await Database.StringGetAsync(RedisNamespace + key)
            );
        }

        

        public async Task<bool> SetObject<T>(string key, T value, TimeSpan interval) where T : class
        {
            return await RedisExecFunction<bool>(async () =>
            {
                try
                {
                    var type = typeof(T);

                    var properties = GetProperties(type);

                    HashEntry[] hashes = new HashEntry[properties.Count];

                    int i = 0;
                    foreach (var p in properties)
                    {
                        hashes[i] = new HashEntry(p.Key, _GetRedisValue[p.Value.PropertyType](p.Value.GetValue(value)));
                        ++i;
                    }

                    var k = RedisNamespace + key;
                    await Database.HashSetAsync(k, hashes);
                    await Database.KeyExpireAsync(k, interval);

                    return true;
                }
                catch (Exception exc)
                {
                    Logger.Instance.Save(exc);
                }
                
                return false;
            }
            );
        }

        public async Task<T> GetObject<T>(string key) where T : class
        {
            return await RedisExecFunction<T>(async () =>
            {
                try
                {
                    var type = typeof(T);

                    var properties = GetProperties(type);

                    var instance = (T)Activator.CreateInstance(type);

                    var hash = await Database.HashGetAllAsync(RedisNamespace + key);

                    foreach (var h in hash)
                    {
                        if (properties.TryGetValue(h.Name, out var property))
                        {
                            _SetRedisValue[property.PropertyType](h.Value, property, instance);
                        }
                    }

                    return instance;
                }
                catch (Exception exc)
                {
                    Logger.Instance.Save(exc);
                }

                return null;
            }
            );
        }

        #endregion Public  method(s)

    }
}
