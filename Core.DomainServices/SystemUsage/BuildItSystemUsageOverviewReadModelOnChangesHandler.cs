using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
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
    IDomainEventHandler<EntityUpdatedEvent<ItSystem>>
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

            _readModelRepository.Add(model);
        }

        public void Handle(EntityUpdatedEvent<ItSystemUsage> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage));
        }

        public void Handle(EntityUpdatedEvent<ItSystem> domainEvent)
        {
            _pendingReadModelUpdateRepository.Add(PendingReadModelUpdate.Create(domainEvent.Entity, PendingReadModelUpdateSourceCategory.ItSystemUsage_ItSystem));
        }

        private void BuildFromSource(ItSystemUsageOverviewReadModel model, ItSystemUsage itSystemUsage)
        {
            _readModelUpdate.Apply(itSystemUsage, model);
        }
    }
}
