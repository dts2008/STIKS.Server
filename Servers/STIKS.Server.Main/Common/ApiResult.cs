using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STIKS.Server
{
    public sealed class ApiResult : Dictionary<string, object>
    {
        public const string status_ok = "OK";

        public const string status_error = "ERROR";

        public Dictionary<string, object> Data
        {
            get { return this; }
        }

        public ApiResult() { this["cmd"] = "init"; }

        public ApiResult(string c)
        {
            this["cmd"] = c;
            this["time"] = (int)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            this["status"] = status_ok;
            this["status_code"] = 0;
        }

        public void Error()
        {
            this["status"] = status_error;
            this["status_code"] = -1;
        }


    }
}
