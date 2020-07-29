using System.Web.Caching;
using Serilog;

namespace Core.ApplicationServices.Authorization
{
    public class CachingUserContextFactory : IUserContextFactory //TODO: Purge the cache for the affected user when roles / rights change (GlobalAdmin flad, Org roles and Project, System and Contract roles)
    {
        private readonly IUserContextFactory _userContextFactory;
        private readonly Cache _cache; //TODO: Wrap that interface with something simple
        private readonly ILogger _logger;

        public CachingUserContextFactory(IUserContextFactory userContextFactory, Cache cache, ILogger logger)
        {
            _userContextFactory = userContextFactory;
            _cache = cache;
            _logger = logger;
        }

        public IOrganizationalUserContext Create(int userId, int organizationId)
        {
            //var key = $"{nameof(IOrganizationalUserContext)}_{userId}";
            //var cachedContext = (IOrganizationalUserContext)_cache.Get(key);
            //if (cachedContext == null)
            //{
            //    _logger.Information("GETTING NEW ONE"); //TODO: Remove
            //    cachedContext = _userContextFactory.Create(userId, organizationId);
            //    _cache.Insert(key, cachedContext, null, DateTime.UtcNow.AddSeconds(5), Cache.NoSlidingExpiration);
            //}
            //else
            //{
            //    _logger.Information("USING CACHED"); //TODO: Remove
            //}
            //return cachedContext;

            //TODO: Once we pre-compute the user context, we can cache it and get rid of the issue where User etc is accessed after the object context behind the object is disposed. Once fixed, re-enable the code above
            return _userContextFactory.Create(userId, organizationId);
        }
    }
}
