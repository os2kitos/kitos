using System;
using Core.DomainModel;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class BuildDataProcessingAgreementReadModelOnChangesHandler :
        IDomainEventHandler<EntityLifeCycleEvent<DataProcessingAgreement>>,
        IDomainEventHandler<EntityLifeCycleEvent<User>>
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

        public void Handle(EntityLifeCycleEvent<DataProcessingAgreement> domain)
        {
            var dataProcessingAgreement = domain.Entity;

            switch (domain.ChangeType)
            {
                case LifeCycleEventType.Created:
                    CreateNewModel(dataProcessingAgreement);
                    break;
                case LifeCycleEventType.Updated:
                    _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(dataProcessingAgreement, PendingReadModelUpdateSourceCategory.DataProcessingAgreement));
                    break;
                case LifeCycleEventType.Deleted:
                    _readModelRepository.DeleteBySourceId(dataProcessingAgreement.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreateNewModel(DataProcessingAgreement dataProcessingAgreement)
        {
            var model = new DataProcessingAgreementReadModel();

            BuildFromSource(model, dataProcessingAgreement);

            _readModelRepository.Add(model);
        }

        private void BuildFromSource(DataProcessingAgreementReadModel model,
            DataProcessingAgreement dataProcessingAgreement)
        {
            _mapper.Apply(dataProcessingAgreement, model);
        }

        public void Handle(EntityLifeCycleEvent<User> domainEvent)
        {
            //Schedule update of affected read models
            if (domainEvent.ChangeType == LifeCycleEventType.Updated)
            {
                _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.DataProcessingAgreement_User));
            }
        }
    }
}
