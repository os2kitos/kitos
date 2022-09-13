using System.Linq;
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
    IDomainEventHandler<EntityUpdatedEvent<Organization>>,
    IDomainEventHandler<EntityBeingDeletedEvent<Organization>>
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
            _readModelRepository.DeleteBySourceId(domainEvent.Entity.Id);

            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_Parent));
        }

        public void Handle(EntityCreatedEvent<ItContract> domainEvent)
        {
            var model = new ItContractOverviewReadModel();

            BuildFromSource(model, domainEvent.Entity);

            _readModelRepository.Add(model); //Add one immediately

            //Schedule additional update to refresh once deferred updates are applied
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract));
        }

        public void Handle(EntityUpdatedEvent<ItContract> domainEvent)
        {
            ScheduleUpdates
            (
                PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract),
                PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_Parent)
            );
        }

        private void BuildFromSource(ItContractOverviewReadModel model, ItContract contract)
        {
            _readModelUpdate.Apply(contract, model);
        }

        public void Handle(EntityUpdatedEvent<OrganizationUnit> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_OrgaizationUnit));
        }

        public void Handle(EntityBeingDeletedEvent<OrganizationUnit> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_OrgaizationUnit));
        }

        public void Handle(EntityUpdatedEvent<CriticalityType> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_CriticalityType));
        }

        public void Handle(EntityUpdatedEvent<Organization> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_Organization));
        }
        public void Handle(EntityBeingDeletedEvent<Organization> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_Organization));
        }

        public void Handle(EntityUpdatedEvent<ItContractType> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_ItContractType));
        }

        public void Handle(EntityUpdatedEvent<ItContractTemplateType> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_ItContractTemplateType));
        }

        public void Handle(EntityUpdatedEvent<PurchaseFormType> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_PurchaseFormType));
        }

        public void Handle(EntityUpdatedEvent<ProcurementStrategyType> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_ProcurementStrategyType));
        }

        public void Handle(EntityUpdatedEvent<User> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_User));
        }

        public void Handle(EntityBeingDeletedEvent<User> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_User));
        }

        public void Handle(EntityUpdatedEvent<DataProcessingRegistration> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_DataProcessingRegistration));
        }

        public void Handle(EntityBeingDeletedEvent<DataProcessingRegistration> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_DataProcessingRegistration));
        }

        public void Handle(EntityUpdatedEvent<ItSystemUsage> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_ItSystemUsage));
        }

        public void Handle(EntityBeingDeletedEvent<ItSystemUsage> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_ItSystemUsage));
        }

        public void Handle(EntityUpdatedEvent<ItSystem> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_ItSystem));
        }

        public void Handle(EntityUpdatedEvent<OptionExtendType> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_OptionExtendType));
        }

        public void Handle(EntityUpdatedEvent<TerminationDeadlineType> domainEvent)
        {
            ScheduleUpdates(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract_TerminationDeadlineType));
        }

        private void ScheduleUpdates(params PendingReadModelUpdate[] pendingReadModelUpdates)
        {
            if (pendingReadModelUpdates.Any())
            {
                _pendingReadModelUpdateRepository.AddMany(pendingReadModelUpdates);
            }
        }
    }
}
