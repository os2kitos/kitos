using Core.DomainModel;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.Contract;

namespace Core.DomainServices.Contract
{
    public class BuildItContractOverviewReadModelOnChangesHandler :
    IDomainEventHandler<EntityBeingDeletedEvent<ItContract>>,
    IDomainEventHandler<EntityCreatedEvent<ItContract>>,
    IDomainEventHandler<EntityUpdatedEvent<ItContract>>,
    IDomainEventHandler<EntityUpdatedEvent<OrganizationUnit>>,
    IDomainEventHandler<EntityBeingDeletedEvent<OrganizationUnit>>,
    IDomainEventHandler<EntityUpdatedEvent<CriticalityType>>,
    IDomainEventHandler<EntityUpdatedEvent<ItContractType>>,
    IDomainEventHandler<EntityUpdatedEvent<PurchaseFormType>>,
    IDomainEventHandler<EntityUpdatedEvent<ProcurementStrategyType>>,
    IDomainEventHandler<EntityUpdatedEvent<ItContractTemplateType>>,
    IDomainEventHandler<EntityUpdatedEvent<OptionExtendType>>,
    IDomainEventHandler<EntityUpdatedEvent<TerminationDeadlineType>>,
    IDomainEventHandler<EntityUpdatedEvent<User>>,
    IDomainEventHandler<EntityBeingDeletedEvent<User>>,
    IDomainEventHandler<EntityUpdatedEvent<ItSystem>>,
    IDomainEventHandler<EntityUpdatedEvent<ItSystemUsage>>,
    IDomainEventHandler<EntityBeingDeletedEvent<ItSystemUsage>>,
    IDomainEventHandler<EntityUpdatedEvent<DataProcessingRegistration>>,
    IDomainEventHandler<EntityBeingDeletedEvent<DataProcessingRegistration>>,
    IDomainEventHandler<EntityUpdatedEvent<Organization>>
    {
    private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;
        private readonly IItContractOverviewReadModelRepository _readModelRepository;
        private readonly IReadModelUpdate<ItContract, ItContractOverviewReadModel> _readModelUpdate;

        public BuildItContractOverviewReadModelOnChangesHandler(
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository, 
            IItContractOverviewReadModelRepository readModelRepository, 
            IReadModelUpdate<ItContract, ItContractOverviewReadModel> readModelUpdate)
        {
            _pendingReadModelUpdateRepository = pendingReadModelUpdateRepository;
            _readModelRepository = readModelRepository;
            _readModelUpdate = readModelUpdate;
        }

        public void Handle(EntityBeingDeletedEvent<ItContract> domainEvent)
        {
            //TODO: Handle by parent
            _readModelRepository.DeleteBySourceId(domainEvent.Entity.Id);
        }

        public void Handle(EntityCreatedEvent<ItContract> domainEvent)
        {
            var model = new ItContractOverviewReadModel();

            BuildFromSource(model, domainEvent.Entity);

            _readModelRepository.Add(model); //Add one immediately

            //Schedule additional update to refresh once deferred updates are applied
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract));
        }

        public void Handle(EntityUpdatedEvent<ItContract> domainEvent)
        {
            //TODO: Handle by parent
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract));
        }

        private void BuildFromSource(ItContractOverviewReadModel model, ItContract contract)
        {
            _readModelUpdate.Apply(contract, model);
        }

        public void Handle(EntityUpdatedEvent<OrganizationUnit> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityBeingDeletedEvent<OrganizationUnit> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<CriticalityType> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<Organization> domainEvent)
        {
            //TODO: Supplier
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<ItContractType> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<ItContractTemplateType> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<PurchaseFormType> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<ProcurementStrategyType> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<User> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityBeingDeletedEvent<User> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<DataProcessingRegistration> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityBeingDeletedEvent<DataProcessingRegistration> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<ItSystemUsage> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityBeingDeletedEvent<ItSystemUsage> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<ItSystem> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<OptionExtendType> domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Handle(EntityUpdatedEvent<TerminationDeadlineType> domainEvent)
        {
            throw new System.NotImplementedException();
        }
    }
}
