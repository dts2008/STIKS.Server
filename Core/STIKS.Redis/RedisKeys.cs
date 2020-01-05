using System;
using System.Collections.Generic;
using System.Text;

namespace STIKS.Redis
{
    public static class RedisKeys
    {
        public static string UserSession = "us.{0}";

        public static string GetUserKey(string session)
        {
            return string.Format(UserSession, session);
        }
    }
}
