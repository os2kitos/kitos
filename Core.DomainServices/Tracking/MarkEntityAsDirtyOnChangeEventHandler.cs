using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Tracking
{
    public class MarkEntityAsDirtyOnChangeEventHandler :
        IDomainEventHandler<EntityUpdatedEvent<ItSystem>>,
        IDomainEventHandler<EntityUpdatedEvent<ItSystemUsage>>,
        IDomainEventHandler<EntityUpdatedEvent<DataProcessingRegistration>>,
        IDomainEventHandler<EntityUpdatedEvent<ItContract>>,
        IDomainEventHandler<EntityUpdatedEvent<ItInterface>>,
        IDomainEventHandler<EntityUpdatedEvent<OrganizationUnit>>
        {
        public void Handle(EntityUpdatedEvent<ItSystem> domainEvent)
        {
            MarkAsDirty(domainEvent.Entity);
        }

        public void Handle(EntityUpdatedEvent<ItSystemUsage> domainEvent)
        {
            MarkAsDirty(domainEvent.Entity);
        }

        public void Handle(EntityUpdatedEvent<DataProcessingRegistration> domainEvent)
        {
            MarkAsDirty(domainEvent.Entity);
        }

        public void Handle(EntityUpdatedEvent<ItContract> domainEvent)
        {
            MarkAsDirty(domainEvent.Entity);
        }

        public void Handle(EntityUpdatedEvent<ItInterface> domainEvent)
        {
            MarkAsDirty(domainEvent.Entity);
        }

        private static void MarkAsDirty(IHasDirtyMarking target)
        {
            target.MarkAsDirty();
        }

        public void Handle(EntityUpdatedEvent<OrganizationUnit> domainEvent)
        {
            MarkAsDirty(domainEvent.Entity);
        }
        }
}
