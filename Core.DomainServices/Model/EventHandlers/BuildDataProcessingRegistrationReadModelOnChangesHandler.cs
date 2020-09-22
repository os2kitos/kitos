using Core.DomainModel;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class BuildDataProcessingRegistrationReadModelOnChangesHandler :
        IDomainEventHandler<EntityDeletedEvent<DataProcessingRegistration>>,
        IDomainEventHandler<EntityCreatedEvent<DataProcessingRegistration>>,
        IDomainEventHandler<EntityUpdatedEvent<DataProcessingRegistration>>,
        IDomainEventHandler<EntityUpdatedEvent<User>>,
        IDomainEventHandler<EntityDeletedEvent<ExternalReference>>,
        IDomainEventHandler<EntityCreatedEvent<ExternalReference>>,
        IDomainEventHandler<EntityUpdatedEvent<ExternalReference>>,
        IDomainEventHandler<NamedEntityChangedNameEvent<ItSystem>>,
        IDomainEventHandler<EnabledStatusChanged<ItSystem>>,
        IDomainEventHandler<EntityUpdatedEvent<Organization>>
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

        public void Handle(EntityDeletedEvent<DataProcessingRegistration> domainEvent)
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
            _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.DataProcessingRegistration));
        }

        public void Handle(EntityUpdatedEvent<User> domainEvent)
        {
            _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_User));
        }

        public void Handle(EntityDeletedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        public void Handle(EntityCreatedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        public void Handle(EntityUpdatedEvent<ExternalReference> domainEvent) => HandleExternalReference(domainEvent);

        private void HandleExternalReference(EntityLifeCycleEvent<ExternalReference> domainEvent)
        {
            //Schedule read model update for affected dpa if dpa was the target of the reference
            var dpa = domainEvent.Entity.DataProcessingRegistration;
            if (dpa != null)
            {
                _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(dpa, PendingReadModelUpdateSourceCategory.DataProcessingRegistration));
            }
        }

        public void Handle(NamedEntityChangedNameEvent<ItSystem> domainEvent)
        {
            _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_ItSystem));
        }

        public void Handle(EnabledStatusChanged<ItSystem> domainEvent)
        {
            _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_ItSystem));
        }

        public void Handle(EntityUpdatedEvent<Organization> domainEvent)
        {
            _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(domainEvent.Entity.Id, PendingReadModelUpdateSourceCategory.DataProcessingRegistration_Organization));
        }
    }
}
