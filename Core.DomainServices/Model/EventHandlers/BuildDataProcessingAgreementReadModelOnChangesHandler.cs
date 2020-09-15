using System;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class BuildDataProcessingAgreementReadModelOnChangesHandler :
        IDomainEventHandler<EntityDeletedEvent<DataProcessingAgreement>>,
        IDomainEventHandler<EntityCreatedEvent<DataProcessingAgreement>>,
        IDomainEventHandler<EntityUpdatedEvent<DataProcessingAgreement>>,
        IDomainEventHandler<EntityUpdatedEvent<User>>,
        IDomainEventHandler<EntityDeletedEvent<ExternalReference>>,
        IDomainEventHandler<EntityCreatedEvent<ExternalReference>>,
        IDomainEventHandler<EntityUpdatedEvent<ExternalReference>>,
        IDomainEventHandler<NamedEntityChangedNameEvent<ItSystem>>
    {
        private readonly IDataProcessingAgreementReadModelRepository _readModelRepository;
        private readonly IReadModelUpdate<DataProcessingAgreement, DataProcessingAgreementReadModel> _mapper;
        private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;

        public BuildDataProcessingAgreementReadModelOnChangesHandler(
            IDataProcessingAgreementReadModelRepository readModelRepository,
            IReadModelUpdate<DataProcessingAgreement, DataProcessingAgreementReadModel> mapper,
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository)
        {
            _readModelRepository = readModelRepository;
            _mapper = mapper;
            _pendingReadModelUpdateRepository = pendingReadModelUpdateRepository;
        }

        private void BuildFromSource(DataProcessingAgreementReadModel model,
            DataProcessingAgreement dataProcessingAgreement)
        {
            _mapper.Apply(dataProcessingAgreement, model);
        }

        public void Handle(EntityDeletedEvent<DataProcessingAgreement> domainEvent)
        {
            _readModelRepository.DeleteBySourceId(domainEvent.Entity.Id);
        }

        public void Handle(EntityCreatedEvent<DataProcessingAgreement> domainEvent)
        {
            var model = new DataProcessingAgreementReadModel();

            BuildFromSource(model, domainEvent.Entity);

            _readModelRepository.Add(model);
        }

        public void Handle(EntityUpdatedEvent<DataProcessingAgreement> domainEvent)
        {
            _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.DataProcessingAgreement));
        }

        public void Handle(EntityUpdatedEvent<User> domainEvent)
        {
            _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.DataProcessingAgreement_User));
        }

        public void Handle(EntityDeletedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        public void Handle(EntityCreatedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        public void Handle(EntityUpdatedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        private void HandleExternalReference(EntityLifeCycleEvent<ExternalReference> domainEvent)
        {
            //Schedule read model update for affected dpa if dpa was the target of the reference
            var dpa = domainEvent.Entity.DataProcessingAgreement;
            if (dpa != null)
            {
                _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(dpa, PendingReadModelUpdateSourceCategory.DataProcessingAgreement));
            }
        }

        public void Handle(NamedEntityChangedNameEvent<ItSystem> domainEvent)
        {
            _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.DataProcessingAgreement_ItSystem));
        }
    }
}
