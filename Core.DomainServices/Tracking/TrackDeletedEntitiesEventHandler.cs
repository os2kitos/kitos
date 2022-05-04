using System;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Tracking;
using Core.DomainServices.Context;

namespace Core.DomainServices.Tracking
{
    public class TrackDeletedEntitiesEventHandler :
        IDomainEventHandler<EntityBeingDeletedEvent<ItSystem>>,
        IDomainEventHandler<EntityBeingDeletedEvent<ItSystemUsage>>,
        IDomainEventHandler<EntityBeingDeletedEvent<DataProcessingRegistration>>,
        IDomainEventHandler<EntityBeingDeletedEvent<ItContract>>,
        IDomainEventHandler<EntityBeingDeletedEvent<ItInterface>>,
        IDomainEventHandler<EntityBeingDeletedEvent<ItProject>>
    {
        private readonly IGenericRepository<LifeCycleTrackingEvent> _trackingEventsRepository;
        private readonly Maybe<ActiveUserIdContext> _userContext;

        public TrackDeletedEntitiesEventHandler(IGenericRepository<LifeCycleTrackingEvent> trackingEventsRepository, Maybe<ActiveUserIdContext> userContext)
        {
            _trackingEventsRepository = trackingEventsRepository;
            _userContext = userContext;
        }

        public void Handle(EntityBeingDeletedEvent<ItSystem> domainEvent) => TrackDeleted(domainEvent.Entity);

        public void Handle(EntityBeingDeletedEvent<ItSystemUsage> domainEvent) => TrackDeleted(domainEvent.Entity);

        public void Handle(EntityBeingDeletedEvent<DataProcessingRegistration> domainEvent) => TrackDeleted(domainEvent.Entity);

        public void Handle(EntityBeingDeletedEvent<ItContract> domainEvent) => TrackDeleted(domainEvent.Entity);

        public void Handle(EntityBeingDeletedEvent<ItInterface> domainEvent) => TrackDeleted(domainEvent.Entity);

        public void Handle(EntityBeingDeletedEvent<ItProject> domainEvent) => TrackDeleted(domainEvent.Entity);

        private void TrackDeleted<T>(T entity) where T : IHasUuid
        {
            var newEvent = CreateEvent(entity);
            newEvent.UserId = _userContext.Select(x => x.ActiveUserId).Match(id => id, () => (int?)null);
            _trackingEventsRepository.Insert(newEvent);
            _trackingEventsRepository.Save();
        }

        private static LifeCycleTrackingEvent CreateEvent(object trackedEntity)
        {
            return trackedEntity switch
            {
                ItContract contract => new LifeCycleTrackingEvent(TrackedLifeCycleEventType.Deleted, contract.Uuid, TrackedEntityType.ItContract, contract.Organization),
                ItProject project => new LifeCycleTrackingEvent(TrackedLifeCycleEventType.Deleted, project.Uuid, TrackedEntityType.ItProject, project.Organization),
                ItSystem system => new LifeCycleTrackingEvent(TrackedLifeCycleEventType.Deleted, system.Uuid, TrackedEntityType.ItSystem, system.Organization, system.AccessModifier, system.BelongsTo),
                ItSystemUsage systemUsage => new LifeCycleTrackingEvent(TrackedLifeCycleEventType.Deleted, systemUsage.Uuid, TrackedEntityType.ItSystemUsage, systemUsage.Organization),
                DataProcessingRegistration dpr => new LifeCycleTrackingEvent(TrackedLifeCycleEventType.Deleted, dpr.Uuid, TrackedEntityType.DataProcessingRegistration, dpr.Organization),
                ItInterface itInterface => new LifeCycleTrackingEvent(TrackedLifeCycleEventType.Deleted, itInterface.Uuid, TrackedEntityType.ItInterface, itInterface.Organization, itInterface.AccessModifier, itInterface.ExhibitedBy?.ItSystem?.BelongsTo),
                _ => throw new ArgumentOutOfRangeException(nameof(trackedEntity), "Entity type not mapped to tracked event type")
            };
        }
    }
}
