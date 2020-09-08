using System;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class BuildDataProcessingAgreementReadModelOnChangesHandler : IDomainEventHandler<EntityLifeCycleEvent<DataProcessingAgreement>>
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
                    _pendingReadModelUpdateRepository.AddIfNotPresent(PendingReadModelUpdate.From(dataProcessingAgreement));
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
    }
}
