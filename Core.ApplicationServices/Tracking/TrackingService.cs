using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Tracking;
using Core.DomainServices;
using Core.DomainServices.Authorization;

namespace Core.ApplicationServices.Tracking
{
    public class TrackingService : ITrackingService
    {
        private readonly IGenericRepository<LifeCycleTrackingEvent> _trackingEventsRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationalUserContext _userContext;

        public TrackingService(IGenericRepository<LifeCycleTrackingEvent> trackingEventsRepository, IAuthorizationContext authorizationContext, IOrganizationalUserContext userContext)
        {
            _trackingEventsRepository = trackingEventsRepository;
            _authorizationContext = authorizationContext;
            _userContext = userContext;
        }

        public IQueryable<LifeCycleTrackingEvent> QueryLifeCycleEvents(TrackedLifeCycleEventType? eventType = null, TrackedEntityType? trackedEntityType = null, DateTime? since = null)
        {
            var query = QueryAllAvailable();

            if (eventType.HasValue)
            {
                var trackedEventType = eventType.Value;
                query = query.Where(x => x.EventType == trackedEventType);
            }
            if (since.HasValue)
            {
                var occurredAt = since.Value.ToUniversalTime();
                query = query.Where(x => x.OccurredAtUtc >= occurredAt);
            }
            if (trackedEntityType.HasValue)
            {
                var entityType = trackedEntityType.Value;
                query = query.Where(x => x.EntityType == entityType);
            }

            return query;
        }

        private IQueryable<LifeCycleTrackingEvent> QueryAllAvailable()
        {
            var query = _trackingEventsRepository.AsQueryable();

            var accessLevel = _authorizationContext.GetCrossOrganizationReadAccess();

            if (accessLevel < CrossOrganizationDataReadAccessLevel.All)
            {
                var organizationIds = _userContext.OrganizationIds.ToList();

                if (accessLevel == CrossOrganizationDataReadAccessLevel.RightsHolder)
                {
                    var rightsHolderAccessOrgIds =
                        _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess).ToList();
                    query = query
                        .Where(x =>
                            (x.OptionalRightsHolderOrganization != null &&
                             rightsHolderAccessOrgIds.Contains(x.OptionalRightsHolderOrganization.Id)) ||
                            (x.OptionalOrganizationReference != null &&
                             organizationIds.Contains(x.OptionalOrganizationReference.Id)));
                }
                else if (accessLevel == CrossOrganizationDataReadAccessLevel.Public)
                {
                    query = query.Where(x =>
                        x.OptionalOrganizationReference == null ||
                        organizationIds.Contains(x.OptionalOrganizationReference.Id) ||
                        x.OptionalAccessModifier == AccessModifier.Public);
                }
                else
                {
                    query = query.Where(x =>
                        x.OptionalOrganizationReference == null ||
                        organizationIds.Contains(x.OptionalOrganizationReference.Id)
                    );
                }
            }

            return query;
        }
    }
}
