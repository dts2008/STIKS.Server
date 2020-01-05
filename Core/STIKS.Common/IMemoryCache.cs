using System;

namespace STIKS.Common
{
    public interface IMemoryCache
    {
        string Get(string key);

        string Set(string key);
    }
}
