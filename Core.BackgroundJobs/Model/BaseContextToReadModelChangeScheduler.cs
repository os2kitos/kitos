using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices.Repositories.BackgroundJobs;
using Infrastructure.Services.DataAccess;

namespace Core.BackgroundJobs.Model
{
    public abstract class BaseContextToReadModelChangeScheduler : IAsyncBackgroundJob
    {
        private readonly IPendingReadModelUpdateRepository _updateRepository;
        private readonly ITransactionManager _transactionManager;
        protected PendingReadModelUpdateSourceCategory RootUpdateCategory { get; }
        public string Id { get; }

        protected BaseContextToReadModelChangeScheduler(
            string id,
            PendingReadModelUpdateSourceCategory rootUpdateCategory, 
            ITransactionManager transactionManager, 
            IPendingReadModelUpdateRepository updateRepository)
        {
            RootUpdateCategory = rootUpdateCategory;
            Id = id;
            _transactionManager = transactionManager;
            _updateRepository = updateRepository;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var updatesExecuted = 0;
            var idsAlreadyScheduled = _updateRepository
                .GetMany(PendingReadModelUpdateSourceCategory.ItContract, int.MaxValue)
                .Select(x => x.SourceId)
                .ToList();

            var alreadyScheduledIds = new HashSet<int>(idsAlreadyScheduled);

            updatesExecuted += ProjectDependencyChangesToRoot(alreadyScheduledIds, token);

            return Task.FromResult(Result<string, OperationError>.Success($"Completed {updatesExecuted} updates"));
        }
        protected abstract int ProjectDependencyChangesToRoot(HashSet<int> alreadyScheduledIds, CancellationToken token);

        #region common stuff 3

        protected int ScheduleRootEntityChanges(
            CancellationToken token, 
            HashSet<int> alreadyScheduledIds, 
            PendingReadModelUpdateSourceCategory childChangeType, 
            Func<PendingReadModelUpdate, IQueryable<ItContractOverviewReadModel>> getQuery)
        {
            var updatesExecuted = 0;
            foreach (var update in _updateRepository.GetMany(childChangeType, int.MaxValue).ToList())
            {
                if (token.IsCancellationRequested)
                    break;

                using var transaction = _transactionManager.Begin();

                var ids = getQuery(update).Select(x => x.SourceEntityId).ToList();

                PerformUpdate(alreadyScheduledIds, ids, update, transaction);
                updatesExecuted++;
            }

            return updatesExecuted;
        }

        protected void PerformUpdate(
            HashSet<int> alreadyScheduledIds,
            IEnumerable<int> idsOfAffectedUsages,
            PendingReadModelUpdate sourceUpdate,
            IDatabaseTransaction transaction)
        {
            var updates = idsOfAffectedUsages
                .Where(id => alreadyScheduledIds.Contains(id) == false)
                .ToList()
                .Select(id => PendingReadModelUpdate.Create(id, RootUpdateCategory))
                .ToList();

            CompleteUpdate(updates, sourceUpdate, transaction);
            updates.ForEach(completedUpdate => alreadyScheduledIds.Add(completedUpdate.SourceId));
        }

        private void CompleteUpdate(List<PendingReadModelUpdate> updates, PendingReadModelUpdate userUpdate, IDatabaseTransaction transaction)
        {
            updates.ForEach(update => _updateRepository.Add(update));
            _updateRepository.Delete(userUpdate);
            transaction.Commit();
        }
        #endregion common stuff 3
    }
}
