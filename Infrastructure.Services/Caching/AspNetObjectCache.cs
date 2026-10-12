using System;
using System.Runtime.Caching;
using Core.Abstractions.Caching;
using Core.Abstractions.Types;


namespace Infrastructure.Services.Caching
{
    public class AspNetObjectCache : IObjectCache
    {
        private static readonly MemoryCache InternalCache = new MemoryCache(nameof(AspNetObjectCache));

        public void Write<T>(T entry, string key, TimeSpan duration) where T : class
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (key == null) throw new ArgumentNullException(nameof(key));

            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.Add(duration) };
            InternalCache.Set(key, entry, policy);
        }

        public void Clear(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            InternalCache.Remove(key);
        }

        public Maybe<T> Read<T>(string key) where T : class
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            return InternalCache.Get(key) as T;
        }
    }
}
