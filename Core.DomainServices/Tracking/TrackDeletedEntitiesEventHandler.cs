using System;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Tracking;

namespace Core.DomainServices.Tracking
{
    public class TrackDeletedEntitiesEventHandler :
        IDomainEventHandler<EntityDeletedEvent<ItSystem>>,
        IDomainEventHandler<EntityDeletedEvent<ItSystemUsage>>,
        IDomainEventHandler<EntityDeletedEvent<DataProcessingRegistration>>,
        IDomainEventHandler<EntityDeletedEvent<ItContract>>,
        IDomainEventHandler<EntityDeletedEvent<ItInterface>>,
        IDomainEventHandler<EntityDeletedEvent<ItProject>>
    {
        private readonly IGenericRepository<LifeCycleTrackingEvent> _trackingEventsRepository;

        public TrackDeletedEntitiesEventHandler(IGenericRepository<LifeCycleTrackingEvent> trackingEventsRepository)
        {
            _trackingEventsRepository = trackingEventsRepository;
        }

        public void Handle(EntityDeletedEvent<ItSystem> domainEvent) => TrackDeleted(domainEvent.Entity);

        public void Handle(EntityDeletedEvent<ItSystemUsage> domainEvent) => TrackDeleted(domainEvent.Entity);

        public void Handle(EntityDeletedEvent<DataProcessingRegistration> domainEvent) => TrackDeleted(domainEvent.Entity);

        public void Handle(EntityDeletedEvent<ItContract> domainEvent) => TrackDeleted(domainEvent.Entity);

        public void Handle(EntityDeletedEvent<ItInterface> domainEvent) => TrackDeleted(domainEvent.Entity);

        public void Handle(EntityDeletedEvent<ItProject> domainEvent) => TrackDeleted(domainEvent.Entity);

        private void TrackDeleted<T>(T entity) where T : IHasUuid
        {
            var newEvent = CreateEvent(entity);
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
