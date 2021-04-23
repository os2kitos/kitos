using Core.DomainModel;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.SystemUsage
{
    public class BuildItSystemUsageOverviewReadModelOnChangesHandler :
    IDomainEventHandler<EntityDeletedEvent<ItSystemUsage>>,
    IDomainEventHandler<EntityCreatedEvent<ItSystemUsage>>,
    IDomainEventHandler<EntityUpdatedEvent<ItSystemUsage>>,
    IDomainEventHandler<EntityUpdatedEvent<ItSystem>>,
    IDomainEventHandler<EntityUpdatedEvent<User>>,
    IDomainEventHandler<EntityUpdatedEvent<OrganizationUnit>>,
    IDomainEventHandler<EntityDeletedEvent<OrganizationUnit>>,
    IDomainEventHandler<EntityUpdatedEvent<Organization>>,
    IDomainEventHandler<EntityDeletedEvent<Organization>>,
    IDomainEventHandler<EntityUpdatedEvent<BusinessType>>,
    IDomainEventHandler<EntityCreatedEvent<LocalBusinessType>>,
    IDomainEventHandler<EntityUpdatedEvent<LocalBusinessType>>,
    IDomainEventHandler<EntityUpdatedEvent<TaskRef>>,
    IDomainEventHandler<EntityDeletedEvent<ExternalReference>>,
    IDomainEventHandler<EntityCreatedEvent<ExternalReference>>,
    IDomainEventHandler<EntityUpdatedEvent<ExternalReference>>,
    IDomainEventHandler<EntityUpdatedEvent<ItContract>>,
    IDomainEventHandler<ContractDeleted>,
    IDomainEventHandler<EntityUpdatedEvent<ItProject>>,
    IDomainEventHandler<EntityDeletedEvent<ItProject>>,
    IDomainEventHandler<EntityUpdatedEvent<DataProcessingRegistration>>,
    IDomainEventHandler<EntityDeletedEvent<DataProcessingRegistration>>,
    IDomainEventHandler<EntityCreatedEvent<SystemRelation>>,
    IDomainEventHandler<EntityUpdatedEvent<SystemRelation>>,
    IDomainEventHandler<EntityDeletedEvent<SystemRelation>>,
    IDomainEventHandler<EntityUpdatedEvent<ItInterface>>,
    IDomainEventHandler<EntityDeletedEvent<ItInterface>>,
    IDomainEventHandler<ExposingSystemChanged>
    {
        private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;
        private readonly IItSystemUsageOverviewReadModelRepository _readModelRepository;
        private readonly IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel> _readModelUpdate;

        public BuildItSystemUsageOverviewReadModelOnChangesHandler(
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository,
            IItSystemUsageOverviewReadModelRepository readModelRepository,
            IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel> readModelUpdate)
        {
            _pendingReadModelUpdateRepository = pendingReadModelUpdateRepository;
            _readModelRepository = readModelRepository;
            _readModelUpdate = readModelUpdate;
        }

        public void Handle(EntityDeletedEvent<ItSystemUsage> domainEvent)
        {
            _readModelRepository.DeleteBySourceId(domainEvent.Entity.Id);
        }

        public void Handle(EntityCreatedEvent<ItSystemUsage> domainEvent)
        {
            var model = new ItSystemUsageOverviewReadModel();

            BuildFromSource(model, domainEvent.Entity);

            _readModelRepository.Add(model); //Add one immediately

            //Schedule additional update to refresh once deferred updates are applied
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage));
        }

        public void Handle(EntityUpdatedEvent<ItSystemUsage> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage));
        }

        public void Handle(EntityUpdatedEvent<ItSystem> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage_ItSystem));
        }

        public void Handle(EntityUpdatedEvent<User> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage_User));
        }

        public void Handle(EntityUpdatedEvent<OrganizationUnit> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage_OrganizationUnit));
        }

        public void Handle(EntityDeletedEvent<OrganizationUnit> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage_OrganizationUnit));
        }

        public void Handle(EntityUpdatedEvent<Organization> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage_Organization));
        }

        public void Handle(EntityDeletedEvent<Organization> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage_Organization));
        }

        public void Handle(EntityUpdatedEvent<BusinessType> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_BusinessType));
        }

        public void Handle(EntityCreatedEvent<LocalBusinessType> domainEvent)
        {
            //Point to parent id since that's what the readmodel knows about
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.OptionId, PendingReadModelUpdateSourceCategory.ItSystemUsage_BusinessType));
        }

        public void Handle(EntityUpdatedEvent<LocalBusinessType> domainEvent)
        {
            //Point to parent id since that's what the readmodel knows about
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.OptionId, PendingReadModelUpdateSourceCategory.ItSystemUsage_BusinessType));
        }

        public void Handle(EntityUpdatedEvent<TaskRef> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage_TaskRef));
        }

        public void Handle(EntityUpdatedEvent<ItContract> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_Contract));
        }

        public void Handle(ContractDeleted domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.DeletedContract.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_Contract));
        }

        public void Handle(EntityUpdatedEvent<ItProject> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_Project));
        }

        public void Handle(EntityDeletedEvent<ItProject> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_Project));
        }

        public void Handle(EntityUpdatedEvent<DataProcessingRegistration> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_DataProcessingRegistration));
        }

        public void Handle(EntityDeletedEvent<DataProcessingRegistration> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_DataProcessingRegistration));
        }

        public void Handle(EntityUpdatedEvent<ItInterface> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_ItInterface));
        }

        public void Handle(EntityDeletedEvent<ItInterface> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_ItInterface));
        }

        public void Handle(ExposingSystemChanged domainEvent)
        {
            //Schedule read model update based on ItSystem if the system was the previous exposer of an interface
            var oldSystem = domainEvent.PreviousSystem;
            if(oldSystem.HasValue)
            {
                _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(oldSystem.Value, PendingReadModelUpdateSourceCategory.ItSystemUsage_ItSystem));
            }

            //Schedule read model update based on ItSystem for the new system exposer of an interface
            var newSystem = domainEvent.NewSystem;
            if(newSystem.HasValue)
            {
                _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(newSystem.Value, PendingReadModelUpdateSourceCategory.ItSystemUsage_ItSystem));
            }
        }

        public void Handle(EntityDeletedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        public void Handle(EntityCreatedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        public void Handle(EntityUpdatedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        private void HandleExternalReference(EntityLifeCycleEvent<ExternalReference> domainEvent)
        {
            //Schedule read model update for affected ItSystemUsage if ItSystemUsage was the target of the reference
            var itSystemUsage = domainEvent.Entity.ItSystemUsage;
            if (itSystemUsage != null)
            {
                _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(itSystemUsage, PendingReadModelUpdateSourceCategory.ItSystemUsage));
            }
        }

        public void Handle(EntityDeletedEvent<SystemRelation> domainEvent) => HandleSystemRelation(domainEvent);

        public void Handle(EntityCreatedEvent<SystemRelation> domainEvent) => HandleSystemRelation(domainEvent);

        public void Handle(EntityUpdatedEvent<SystemRelation> domainEvent) => HandleSystemRelation(domainEvent);

        private void HandleSystemRelation(EntityLifeCycleEvent<SystemRelation> domainEvent)
        {
            //Schedule read model update for fromSystemUsage 
            var fromSystemUsage = domainEvent.Entity.FromSystemUsage;
            if (fromSystemUsage != null)
            {
                _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(fromSystemUsage, PendingReadModelUpdateSourceCategory.ItSystemUsage));
            }

            //Schedule read model update for toSystemUsage 
            var toSystemUsage = domainEvent.Entity.ToSystemUsage;
            if (toSystemUsage != null)
            {
                _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(toSystemUsage, PendingReadModelUpdateSourceCategory.ItSystemUsage));
            }
        }


        private void BuildFromSource(ItSystemUsageOverviewReadModel model, ItSystemUsage itSystemUsage)
        {
            _readModelUpdate.Apply(itSystemUsage, model);
        }
    }
}
