using System;
using System.Data;
using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Serilog;

namespace Core.DomainServices.Model.EventHandlers
{
    public class UpdateRelationsOnSystemUsageDeletedHandler : IDomainEventHandler<EntityDeletedEvent<ItSystemUsage>>
    {
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly IGenericRepository<SystemRelation> _systemRelationRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;

        public UpdateRelationsOnSystemUsageDeletedHandler(
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            IGenericRepository<SystemRelation> systemRelationRepository,
            ITransactionManager transactionManager,
            ILogger logger)
        {
            _systemUsageRepository = systemUsageRepository;
            _systemRelationRepository = systemRelationRepository;
            _transactionManager = transactionManager;
            _logger = logger;
        }

        public void Handle(EntityDeletedEvent<ItSystemUsage> domainEvent)
        {
            if (domainEvent == null)
                throw new ArgumentNullException(nameof(domainEvent));

            if (domainEvent.ChangeType != LifeCycleEventType.Deleted)
                return;

            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            var deletedSystemUsage = domainEvent.Entity;

            _logger.Debug("System usage with id {id} deleted. All relations TO from other usages will be removed",
                deletedSystemUsage.Id);

            //Delete all relations which point TO the deleted system usage
            var usedByRelationsByFromUsage =
                deletedSystemUsage
                    .UsedByRelations
                    .GroupBy(relation => relation.FromSystemUsageId)
                    .ToList();

            foreach (var systemRelationsByFromUsage in usedByRelationsByFromUsage)
            {
                var fromSystemUsage = systemRelationsByFromUsage.First().FromSystemUsage;

                foreach (var relationToBeRemoved in systemRelationsByFromUsage)
                {
                    var relationId = relationToBeRemoved.Id;
                    var result = fromSystemUsage.RemoveUsageRelation(relationId);
                    if (result.Failed)
                    {
                        throw new InvalidOperationException(
                            $"Failed to remove relation with id {relationId} from system usage with id {fromSystemUsage.Id}. Reported error:{result.Error:G}");
                    }

                    _systemRelationRepository.Delete(relationToBeRemoved);
                }
            }

            _systemUsageRepository.Save();
            _systemRelationRepository.Save();
            transaction.Commit();
        }
    }
}
