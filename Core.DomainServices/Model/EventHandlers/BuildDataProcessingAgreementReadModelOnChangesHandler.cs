using System;
using Core.DomainModel.GDPR;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class BuildDataProcessingAgreementReadModelOnChangesHandler : IDomainEventHandler<EntityLifeCycleEvent<DataProcessingAgreement>>
    {
        private readonly IDataProcessingAgreementReadModelRepository _readModelRepository;

        public BuildDataProcessingAgreementReadModelOnChangesHandler(IDataProcessingAgreementReadModelRepository readModelRepository)
        {
            _readModelRepository = readModelRepository;
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
                    var readModel = _readModelRepository.GetBySourceId(dataProcessingAgreement.Id);
                    if (readModel.HasValue)
                    {
                        //Update the existing model
                        var readModelValue = readModel.Value;
                        BuildFromSource(readModelValue, dataProcessingAgreement);
                        _readModelRepository.Update(readModelValue);
                    }
                    else
                    {
                        //Not created yet - build it
                        CreateNewModel(dataProcessingAgreement);
                    }
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

        private static void BuildFromSource(DataProcessingAgreementReadModel model,
            DataProcessingAgreement dataProcessingAgreement)
        {
            model.OrganizationId = dataProcessingAgreement.OrganizationId;
            model.SourceEntityId = dataProcessingAgreement.Id;
            model.Name = dataProcessingAgreement.Name;
        }
    }
}
