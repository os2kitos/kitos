using System;
using System.Data;
using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.DomainEvents;
using Core.DomainModel.Result;
using Core.DomainServices.Context;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Serilog;

namespace Core.DomainServices.Model.EventHandlers
{
    public class SystemUsageDeletedHandler : IDomainEventHandler<SystemUsageDeleted>
    {
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly IGenericRepository<SystemRelation> _systemRelationRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IOperationClock _clock;
        private readonly ILogger _logger;
        private readonly Maybe<ActiveUserContext> _userContext;

        public SystemUsageDeletedHandler(
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            IGenericRepository<SystemRelation> systemRelationRepository,
            ITransactionManager transactionManager,
            IOperationClock clock,
            ILogger logger,
            Maybe<ActiveUserContext> userContext)
        {
            _systemUsageRepository = systemUsageRepository;
            _systemRelationRepository = systemRelationRepository;
            _transactionManager = transactionManager;
            _clock = clock;
            _logger = logger;
            _userContext = userContext;
        }

        public void Handle(SystemUsageDeleted domainEvent)
        {
            if (domainEvent == null)
                throw new ArgumentNullException(nameof(domainEvent));

            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                var deletedSystemUsage = domainEvent.DeletedSystemUsage;
                var updateTime = _clock.Now;

                //Delete all relations which point TO the deleted system usage
                var usedByRelationsByFromUsage =
                    deletedSystemUsage
                        .UsedByRelations
                        .GroupBy(relation => relation.FromSystemUsageId)
                        .ToList();

                foreach (var systemRelationsByFromUsage in usedByRelationsByFromUsage)
                {
                    var fromSystemUsage = systemRelationsByFromUsage.First().FromSystemUsage;
                    var activeUser = _userContext.Match(ctx => ctx.UserEntity, () => fromSystemUsage.ObjectOwner);

                    foreach (var relationToBeRemoved in systemRelationsByFromUsage)
                    {
                        var relationId = relationToBeRemoved.Id;
                        var result = fromSystemUsage.RemoveUsageRelation(relationId);
                        if (result.Failed)
                        {
                            throw new InvalidOperationException($"Failed to remove relation with id {relationId} from system usage with id {fromSystemUsage.Id}");
                        }
                        _systemRelationRepository.Delete(relationToBeRemoved);
                    }

                    fromSystemUsage.LastChangedByUser = activeUser;
                    fromSystemUsage.LastChanged = updateTime;
                }

                _systemUsageRepository.Save();
                _systemRelationRepository.Save();
                transaction.Commit();
            }
        }
    }
}
