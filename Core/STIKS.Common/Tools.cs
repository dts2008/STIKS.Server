using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace STIKS.Common
{
    public class Tools
    {
        static public T Deserialize<T>(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data)) return default(T);

                return JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception exc)
            {
                Logger.Instance.Save(exc);
            }

            return default(T);
        }

        static public string ToJson(object o)
        {
            try
            {
                return JsonSerializer.Serialize(o);
            }
            catch (Exception exc)
            {
                Logger.Instance.Save(exc);
            }

            return string.Empty;
        }

        static public int ToInt(object s)
        {
            try
            {
                if (s == null) return 0;
                return Convert.ToInt32(s);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        static public string GetMD5(string str)
        {
            var md5 = new MD5CryptoServiceProvider();
            return ToHex(md5.ComputeHash(Encoding.ASCII.GetBytes(str)));
        }

        static public string ToHex(byte[] data)
        {
            var hash = new StringBuilder();
            for (int i = 0; i < 16; ++i) hash.AppendFormat("{0:x2}", data[i]);

            return hash.ToString();
        }
    }
}
