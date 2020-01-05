using STIKS.Common;
using STIKS.DBManager;
using STIKS.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STIKS.Server
{
    public class UserEngines
    {
        public static UserEngines Instance { get; } = new UserEngines();
        public async Task<ApiResult> Authorization(string login, string hash, int time)
        {
            var result = new ApiResult("authorization");

            var user = UserManager.Instance.ByLogin(login);
            if (user == null || user.Password != hash)
            {
                result.Error();
                return result;
            }

            string session = Guid.NewGuid().ToString("N");

            await RedisCacheEngine.Instance.SetObject(RedisKeys.GetUserKey(session), user, RedisCacheEngine.DefExpirationTime);
            
            result["session"] = session;

            return result;
        }
    }
}
