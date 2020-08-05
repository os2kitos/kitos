using System;
using System.Web.Caching;
using Infrastructure.Services.Types;

namespace Infrastructure.Services.Caching
{
    public class AspNetObjectCache : IObjectCache
    {
        private readonly Cache _internalCache;

        public AspNetObjectCache()
        {
            _internalCache = new Cache();
        }

        public void Write<T>(T entry, string key, TimeSpan duration) where T : class
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (key == null) throw new ArgumentNullException(nameof(key));

            _internalCache.Insert(key, entry, null, DateTime.UtcNow.Add(duration), Cache.NoSlidingExpiration);
        }

        public void Clear(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            _internalCache.Remove(key);
        }

        public Maybe<T> Read<T>(string key) where T : class
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            return _internalCache.Get(key) as T;
        }
    }
}
