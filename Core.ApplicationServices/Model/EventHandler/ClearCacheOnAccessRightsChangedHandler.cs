using Core.ApplicationServices.Authorization;
using Core.DomainModel.Organization.DomainEvents;
using Infrastructure.Services.Caching;
using Infrastructure.Services.DomainEvents;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ClearCacheOnAccessRightsChangedHandler : IDomainEventHandler<AccessRightsChanged>
    {
        private readonly IObjectCache _objectCache;

        public ClearCacheOnAccessRightsChangedHandler(IObjectCache objectCache)
        {
            _objectCache = objectCache;
        }

        public void Handle(AccessRightsChanged domainEvent)
        {
            _objectCache.Clear(OrganizationalUserContextCacheKeyFactory.Create(domainEvent.UserId));
        }
    }
}
