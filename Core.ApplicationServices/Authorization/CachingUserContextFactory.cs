using System;
using Infrastructure.Services.Caching;

namespace Core.ApplicationServices.Authorization
{
    public class CachingUserContextFactory : IUserContextFactory
    {
        private readonly IUserContextFactory _userContextFactory;
        private readonly IObjectCache _cache;

        public CachingUserContextFactory(IUserContextFactory userContextFactory, IObjectCache cache)
        {
            _userContextFactory = userContextFactory;
            _cache = cache;
        }

        public IOrganizationalUserContext Create(int userId)
        {
            var key = OrganizationalUserContextCacheKeyFactory.Create(userId);
            var cachedContext = _cache.Read<IOrganizationalUserContext>(key);
            if (cachedContext.HasValue)
            {
                return cachedContext.Value;
            }
            var newEntry = _userContextFactory.Create(userId);
            _cache.Write(newEntry, key, TimeSpan.FromMinutes(20));
            return newEntry;
        }
    }
}
