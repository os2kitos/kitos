using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DataAccess;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class ScheduleItSystemUsageOverviewReadModelUpdates : IAsyncBackgroundJob
    {
        private readonly IPendingReadModelUpdateRepository _updateRepository;
        private readonly IItSystemUsageOverviewReadModelRepository _readModelRepository;
        private readonly IItSystemUsageRepository _itSystemUsageRepository;
        private readonly IItSystemRepository _itSystemRepository;
        private readonly ITransactionManager _transactionManager;
        public string Id => StandardJobIds.ScheduleItSystemUsageOverviewReadModelUpdates;
        private const int BatchSize = 250;

        public ScheduleItSystemUsageOverviewReadModelUpdates(
            IPendingReadModelUpdateRepository updateRepository,
            IItSystemUsageOverviewReadModelRepository readModelRepository,
            IItSystemUsageRepository itSystemUsageRepository,
            IItSystemRepository itSystemRepository,
            ITransactionManager transactionManager)
        {
            _updateRepository = updateRepository;
            _readModelRepository = readModelRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
            _itSystemRepository = itSystemRepository;
            _transactionManager = transactionManager;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var updatesExecuted = 0;
            var alreadyScheduledIds = new HashSet<int>();

            updatesExecuted = HandleSystemUpdates(token, updatesExecuted, alreadyScheduledIds);
            updatesExecuted = HandleUserUpdates(token, updatesExecuted, alreadyScheduledIds);
            updatesExecuted = HandleOrganizationUnitUpdated(token, updatesExecuted, alreadyScheduledIds);
            updatesExecuted = HandleOrganizationUpdated(token, updatesExecuted, alreadyScheduledIds);

            return Task.FromResult(Result<string, OperationError>.Success($"Completed {updatesExecuted} updates"));
        }

        private int HandleSystemUpdates(CancellationToken token, int updatesExecuted, HashSet<int> alreadyScheduledIds)
        {
            foreach (var update in _updateRepository.GetMany(PendingReadModelUpdateSourceCategory.ItSystemUsage_ItSystem, BatchSize).ToList())
            {
                if (token.IsCancellationRequested)
                    break;

                using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
                
                //System id is not stored in read model so search the source model
                var systemIds = _itSystemUsageRepository.GetBySystemId(update.SourceId).Select(x=>x.Id);
                var parentSystemIds = _itSystemUsageRepository.GetByParentSystemId(update.SourceId).Select(x => x.Id);
                var ids = systemIds.Concat(parentSystemIds).Distinct();

                updatesExecuted = PerformUpdate(updatesExecuted, alreadyScheduledIds, ids, update, transaction);
            }

            return updatesExecuted;
        }

        private int HandleUserUpdates(CancellationToken token, int updatesExecuted, HashSet<int> alreadyScheduledIds)
        {
            foreach (var update in _updateRepository.GetMany(PendingReadModelUpdateSourceCategory.ItSystemUsage_User, BatchSize).ToList())
            {
                if (token.IsCancellationRequested)
                    break;

                using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
                var ids = _readModelRepository.GetByUserId(update.SourceId).Select(x => x.SourceEntityId);
                updatesExecuted = PerformUpdate(updatesExecuted, alreadyScheduledIds, ids, update, transaction);
            }

            return updatesExecuted;
        }

        private int HandleOrganizationUnitUpdated(CancellationToken token, int updatesExecuted, HashSet<int> alreadyScheduledIds)
        {
            foreach (var update in _updateRepository.GetMany(PendingReadModelUpdateSourceCategory.ItSystemUsage_OrganizationUnit, BatchSize).ToList())
            {
                if (token.IsCancellationRequested)
                    break;

                using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
                var ids = _readModelRepository.GetByOrganizationUnitId(update.SourceId).Select(x => x.SourceEntityId);
                updatesExecuted = PerformUpdate(updatesExecuted, alreadyScheduledIds, ids, update, transaction);
            }

            return updatesExecuted;
        }

        private int HandleOrganizationUpdated(CancellationToken token, int updatesExecuted, HashSet<int> alreadyScheduledIds)
        {
            foreach (var update in _updateRepository.GetMany(PendingReadModelUpdateSourceCategory.ItSystemUsage_Organization, BatchSize).ToList())
            {
                if (token.IsCancellationRequested)
                    break;

                using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
                var systemIds = _itSystemRepository.GetByRightsHolderId(update.SourceId).Select(x => x.Id).ToList();

                var systemUsageIds = _itSystemUsageRepository.GetBySystemIds(systemIds).Select(x => x.Id);
                var systemUsageIdsFromReadModel = _readModelRepository.GetByOrganizationId(update.SourceId).Select(x => x.SourceEntityId);

                var ids = systemUsageIds.Concat(systemUsageIdsFromReadModel).Distinct();

                updatesExecuted = PerformUpdate(updatesExecuted, alreadyScheduledIds, ids, update, transaction);
            }

            return updatesExecuted;
        }


        private int PerformUpdate(
            int updatesExecuted,
            HashSet<int> alreadyScheduledIds,
            IQueryable<int> itSystemUsageIds,
            PendingReadModelUpdate update,
            IDatabaseTransaction transaction)
        {
            var updates = itSystemUsageIds
                .Where(id => alreadyScheduledIds.Contains(id) == false)
                .ToList()
                .Select(id => PendingReadModelUpdate.Create(id, PendingReadModelUpdateSourceCategory.ItSystemUsage))
                .ToList();

            updatesExecuted = CompleteUpdate(updatesExecuted, updates, update, transaction);
            return updatesExecuted;
        }

        private int CompleteUpdate(int updatesExecuted, List<PendingReadModelUpdate> updates, PendingReadModelUpdate userUpdate,
            IDatabaseTransaction transaction)
        {
            updates.ForEach(update => _updateRepository.Add(update));
            _updateRepository.Delete(userUpdate);
            transaction.Commit();
            updatesExecuted++;
            return updatesExecuted;
        }
    }
}
