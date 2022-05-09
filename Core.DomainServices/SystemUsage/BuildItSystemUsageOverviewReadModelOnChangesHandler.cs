﻿using Core.DomainModel;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.SystemUsage;

namespace Core.DomainServices.SystemUsage
{
    public class BuildItSystemUsageOverviewReadModelOnChangesHandler :
    IDomainEventHandler<EntityBeingDeletedEvent<ItSystemUsage>>,
    IDomainEventHandler<EntityCreatedEvent<ItSystemUsage>>,
    IDomainEventHandler<EntityUpdatedEvent<ItSystemUsage>>,
    IDomainEventHandler<EntityUpdatedEvent<ItSystem>>,
    IDomainEventHandler<EntityUpdatedEvent<User>>,
    IDomainEventHandler<EntityUpdatedEvent<OrganizationUnit>>,
    IDomainEventHandler<EntityBeingDeletedEvent<OrganizationUnit>>,
    IDomainEventHandler<EntityUpdatedEvent<Organization>>,
    IDomainEventHandler<EntityBeingDeletedEvent<Organization>>,
    IDomainEventHandler<EntityUpdatedEvent<BusinessType>>,
    IDomainEventHandler<EntityCreatedEvent<LocalBusinessType>>,
    IDomainEventHandler<EntityUpdatedEvent<LocalBusinessType>>,
    IDomainEventHandler<EntityUpdatedEvent<TaskRef>>,
    IDomainEventHandler<EntityBeingDeletedEvent<ExternalReference>>,
    IDomainEventHandler<EntityCreatedEvent<ExternalReference>>,
    IDomainEventHandler<EntityUpdatedEvent<ExternalReference>>,
    IDomainEventHandler<EntityUpdatedEvent<ItContract>>,
    IDomainEventHandler<EntityBeingDeletedEvent<ItContract>>,
    IDomainEventHandler<EntityUpdatedEvent<ItProject>>,
    IDomainEventHandler<EntityBeingDeletedEvent<ItProject>>,
    IDomainEventHandler<EntityUpdatedEvent<DataProcessingRegistration>>,
    IDomainEventHandler<EntityBeingDeletedEvent<DataProcessingRegistration>>,
    IDomainEventHandler<EntityUpdatedEvent<ItInterface>>,
    IDomainEventHandler<EntityBeingDeletedEvent<ItInterface>>
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

        public void Handle(EntityBeingDeletedEvent<ItSystemUsage> domainEvent)
        {
            _readModelRepository.DeleteBySourceId(domainEvent.Entity.Id);
         
            //Related overview models are also affected
            foreach (var entityUsageRelation in domainEvent.Entity.UsageRelations)
            {
                _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(entityUsageRelation.ToSystemUsageId, PendingReadModelUpdateSourceCategory.ItSystemUsage));
            }

            foreach (var entityUsedByRelation in domainEvent.Entity.UsedByRelations)
            {
                _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(entityUsedByRelation.FromSystemUsageId, PendingReadModelUpdateSourceCategory.ItSystemUsage));
            }
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

        public void Handle(EntityBeingDeletedEvent<OrganizationUnit> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage_OrganizationUnit));
        }

        public void Handle(EntityUpdatedEvent<Organization> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage_Organization));
        }

        public void Handle(EntityBeingDeletedEvent<Organization> domainEvent)
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

        public void Handle(EntityBeingDeletedEvent<ItContract> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_Contract));
        }

        public void Handle(EntityUpdatedEvent<ItProject> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_Project));
        }

        public void Handle(EntityBeingDeletedEvent<ItProject> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_Project));
        }

        public void Handle(EntityUpdatedEvent<DataProcessingRegistration> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_DataProcessingRegistration));
        }

        public void Handle(EntityBeingDeletedEvent<DataProcessingRegistration> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_DataProcessingRegistration));
        }

        public void Handle(EntityUpdatedEvent<ItInterface> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_ItInterface));
        }

        public void Handle(EntityBeingDeletedEvent<ItInterface> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.ItSystemUsage_ItInterface));
        }

        public void Handle(EntityBeingDeletedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

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

        private void BuildFromSource(ItSystemUsageOverviewReadModel model, ItSystemUsage itSystemUsage)
        {
            _readModelUpdate.Apply(itSystemUsage, model);
        }
    }
}
