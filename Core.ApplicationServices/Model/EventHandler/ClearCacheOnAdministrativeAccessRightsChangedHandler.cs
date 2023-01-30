using Core.Abstractions.Caching;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Events;
using Core.DomainModel.Organization.DomainEvents;
using Infrastructure.Services.Caching;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ClearCacheOnAdministrativeAccessRightsChangedHandler : IDomainEventHandler<AdministrativeAccessRightsChanged>
    {
        private readonly IObjectCache _objectCache;

        public ClearCacheOnAdministrativeAccessRightsChangedHandler(IObjectCache objectCache)
        {
            _objectCache = objectCache;
        }

        public void Handle(AdministrativeAccessRightsChanged domainEvent)
        {
            _objectCache.Clear(OrganizationalUserContextCacheKeyFactory.Create(domainEvent.UserId));
        }
    }
}
