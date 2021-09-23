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
            var trackedEntityType = GetEntityType(entity);
            var newEvent = new LifeCycleTrackingEvent(entity.Uuid, trackedEntityType, TrackedLifeCycleEventType.Deleted);
            _trackingEventsRepository.Insert(newEvent);
            _trackingEventsRepository.Save();
        }

        private static TrackedEntityType GetEntityType(object trackedEntity)
        {
            return trackedEntity switch
            {
                ItContract => TrackedEntityType.ItContract,
                ItProject => TrackedEntityType.ItProject,
                ItSystem => TrackedEntityType.ItSystem,
                ItSystemUsage => TrackedEntityType.ItSystemUsage,
                DataProcessingRegistration => TrackedEntityType.DataProcessingRegistration,
                ItInterface => TrackedEntityType.ItInterface,
                _ => throw new ArgumentOutOfRangeException(nameof(trackedEntity), "Entity type not mapped to tracked event type")
            };
        }
    }
}
