using System;
using System.Web.Caching;
using Serilog;

namespace Core.ApplicationServices.Authorization
{
    //TODO-MRJ: Purge the cache for the affected user when roles / rights change (GlobalAdmin flad, Org roles and Project, System and Contract roles)
    public class CachingUserContextFactory : IUserContextFactory 
    {
        private readonly IUserContextFactory _userContextFactory;
        private readonly Cache _cache; //TODO-MRJ: Wrap that interface with something simple
        private readonly ILogger _logger;

        public CachingUserContextFactory(IUserContextFactory userContextFactory, Cache cache, ILogger logger)
        {
            _userContextFactory = userContextFactory;
            _cache = cache;
            _logger = logger;
        }

        public IOrganizationalUserContext Create(int userId)
        {
            var key = $"{nameof(IOrganizationalUserContext)}_{userId}";
            var cachedContext = (IOrganizationalUserContext)_cache.Get(key);
            if (cachedContext == null)
            {
                _logger.Information("GETTING NEW ONE"); //TODO-MRJ: Remove
                cachedContext = _userContextFactory.Create(userId);
                _cache.Insert(key, cachedContext, null, DateTime.UtcNow.AddMinutes(1), Cache.NoSlidingExpiration);
            }
            else
            {
                _logger.Information("USING CACHED"); //TODO-MRJ: Remove
            }
            return cachedContext;
        }
    }
}
