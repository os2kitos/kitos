using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.Contract;

namespace Core.DomainServices.Contract
{
    public class BuildItContractOverviewReadModelOnChangesHandler :
    IDomainEventHandler<EntityBeingDeletedEvent<ItContract>>,
    IDomainEventHandler<EntityCreatedEvent<ItContract>>,
    IDomainEventHandler<EntityUpdatedEvent<ItContract>>
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
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItContract));
        }

        private void BuildFromSource(ItContractOverviewReadModel model, ItContract contract)
        {
            _readModelUpdate.Apply(contract, model);
        }
    }
}
