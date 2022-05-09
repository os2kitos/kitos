﻿using System.Linq;
using Core.DomainModel;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;

namespace Core.DomainServices.GDPR
{
    public class BuildDataProcessingRegistrationReadModelOnChangesHandler :
        IDomainEventHandler<EntityBeingDeletedEvent<DataProcessingRegistration>>,
        IDomainEventHandler<EntityCreatedEvent<DataProcessingRegistration>>,
        IDomainEventHandler<EntityUpdatedEvent<DataProcessingRegistration>>,
        IDomainEventHandler<EntityUpdatedEvent<User>>,
        IDomainEventHandler<EntityBeingDeletedEvent<ExternalReference>>,
        IDomainEventHandler<EntityCreatedEvent<ExternalReference>>,
        IDomainEventHandler<EntityUpdatedEvent<ExternalReference>>,
        IDomainEventHandler<NamedEntityChangedNameEvent<ItSystem>>,
        IDomainEventHandler<EnabledStatusChanged<ItSystem>>,
        IDomainEventHandler<EntityUpdatedEvent<Organization>>,
        IDomainEventHandler<EntityUpdatedEvent<DataProcessingBasisForTransferOption>>,
        IDomainEventHandler<EntityUpdatedEvent<LocalDataProcessingBasisForTransferOption>>,
        IDomainEventHandler<EntityUpdatedEvent<DataProcessingDataResponsibleOption>>,
        IDomainEventHandler<EntityUpdatedEvent<LocalDataProcessingDataResponsibleOption>>,
        IDomainEventHandler<EntityUpdatedEvent<DataProcessingOversightOption>>,
        IDomainEventHandler<EntityUpdatedEvent<LocalDataProcessingOversightOption>>,
        IDomainEventHandler<EntityUpdatedEvent<ItContract>>,
        IDomainEventHandler<EntityBeingDeletedEvent<ItContract>>
    {
        private readonly IDataProcessingRegistrationReadModelRepository _readModelRepository;
        private readonly IReadModelUpdate<DataProcessingRegistration, DataProcessingRegistrationReadModel> _mapper;
        private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;

        public BuildDataProcessingRegistrationReadModelOnChangesHandler(
            IDataProcessingRegistrationReadModelRepository readModelRepository,
            IReadModelUpdate<DataProcessingRegistration, DataProcessingRegistrationReadModel> mapper,
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository)
        {
            _readModelRepository = readModelRepository;
            _mapper = mapper;
            _pendingReadModelUpdateRepository = pendingReadModelUpdateRepository;
        }

        private void BuildFromSource(DataProcessingRegistrationReadModel model,
            DataProcessingRegistration dataProcessingRegistration)
        {
            _mapper.Apply(dataProcessingRegistration, model);
        }

        public void Handle(EntityBeingDeletedEvent<DataProcessingRegistration> domainEvent)
        {
            _readModelRepository.DeleteBySourceId(domainEvent.Entity.Id);
        }

        public void Handle(EntityCreatedEvent<DataProcessingRegistration> domainEvent)
        {
            var model = new DataProcessingRegistrationReadModel();

            BuildFromSource(model, domainEvent.Entity);

            _readModelRepository.Add(model);
        }

        public void Handle(EntityUpdatedEvent<DataProcessingRegistration> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.DataProcessingRegistration));
        }

        public void Handle(EntityUpdatedEvent<User> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_User));
        }

        public void Handle(EntityBeingDeletedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        public void Handle(EntityCreatedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        public void Handle(EntityUpdatedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        private void HandleExternalReference(EntityLifeCycleEvent<ExternalReference> domainEvent)
        {
            //Schedule read model update for affected dpa if dpa was the target of the reference
            var dpa = domainEvent.Entity.DataProcessingRegistration;
            if (dpa != null)
            {
                _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(dpa, PendingReadModelUpdateSourceCategory.DataProcessingRegistration));
            }
        }

        public void Handle(NamedEntityChangedNameEvent<ItSystem> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_ItSystem));
        }

        public void Handle(EnabledStatusChanged<ItSystem> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_ItSystem));
        }

        public void Handle(EntityUpdatedEvent<Organization> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_Organization));
        }

        public void Handle(EntityUpdatedEvent<DataProcessingBasisForTransferOption> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_BasisForTransfer));
        }

        public void Handle(EntityUpdatedEvent<LocalDataProcessingBasisForTransferOption> domainEvent)
        {
            //Point to parent id since that's what the dpr knows about
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.OptionId, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_BasisForTransfer));
        }

        public void Handle(EntityUpdatedEvent<DataProcessingDataResponsibleOption> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_DataResponsible));
        }

        public void Handle(EntityUpdatedEvent<LocalDataProcessingDataResponsibleOption> domainEvent)
        {
            //Point to parent id since that's what the dpr knows about
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.OptionId, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_DataResponsible));
        }

        public void Handle(EntityUpdatedEvent<DataProcessingOversightOption> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_OversightOption));
        }

        public void Handle(EntityUpdatedEvent<LocalDataProcessingOversightOption> domainEvent)
        {
            //Point to parent id since that's what the dpr knows about
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.OptionId, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_OversightOption));
        }

        public void Handle(EntityUpdatedEvent<ItContract> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_ItContract));
        }

        public void Handle(EntityBeingDeletedEvent<ItContract> domainEvent)
        {
            domainEvent
                .Entity
                .DataProcessingRegistrations
                .Select(x => PendingReadModelUpdate.Create(x.Id, PendingReadModelUpdateSourceCategory.DataProcessingRegistration))
                .ToList()
                .ForEach(_pendingReadModelUpdateRepository.Add);
        }
    }
}
