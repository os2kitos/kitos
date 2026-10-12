using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.BackgroundJobs;

namespace Core.DomainServices.Contract
{
    public class BuildItContractSupplierOverviewReadModelOnChangesHandler(
        IPendingReadModelUpdateRepository pendingReadModelUpdateRepository)
        :
            IDomainEventHandler<EntityCreatedEvent<ItContract>>,
            IDomainEventHandler<EntityUpdatedEvent<ItContract>>,
            IDomainEventHandler<EntityBeingDeletedEvent<ItContract>>,
            IDomainEventHandler<EntityUpdatedEvent<CriticalityType>>,
            IDomainEventHandler<EntityUpdatedEvent<Organization>>,
            IDomainEventHandler<EntityBeingDeletedEvent<Organization>>
    {
        public void Handle(EntityCreatedEvent<ItContract> domainEvent)
        {
            pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview));
        }

        public void Handle(EntityUpdatedEvent<ItContract> domainEvent)
        {
            pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview));
        }

        public void Handle(EntityBeingDeletedEvent<ItContract> domainEvent)
        {
            pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview));
        }

        public void Handle(EntityUpdatedEvent<CriticalityType> domainEvent)
        {
            pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview_CriticalityType));
        }

        public void Handle(EntityUpdatedEvent<Organization> domainEvent)
        {
            pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview_Organization));
        }

        public void Handle(EntityBeingDeletedEvent<Organization> domainEvent)
        {
            pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview_Organization));
        }
    }
}
