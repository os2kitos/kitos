using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel.BackgroundJobs;
using Core.DomainServices;
using Core.DomainServices.Repositories.BackgroundJobs;
using Infrastructure.Services.DataAccess;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class PurgeDuplicatePendingReadModelUpdates : IAsyncBackgroundJob
    {
        private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IGenericRepository<PendingReadModelUpdate> _primitiveRepository;
        public string Id => StandardJobIds.PurgeDuplicatePendingReadModelUpdates;

        public PurgeDuplicatePendingReadModelUpdates(
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository,
            ITransactionManager transactionManager,
            IGenericRepository<PendingReadModelUpdate> primitiveRepository)
        {
            _pendingReadModelUpdateRepository = pendingReadModelUpdateRepository;
            _transactionManager = transactionManager;
            _primitiveRepository = primitiveRepository;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var deleted = 0;
            foreach (var category in Enum.GetValues(typeof(PendingReadModelUpdateSourceCategory)).Cast<PendingReadModelUpdateSourceCategory>().ToList())
            {
                if (token.IsCancellationRequested)
                    break;

                using var transaction = _transactionManager.Begin();
                var idsInQueue = new HashSet<int>();

                var updates = _pendingReadModelUpdateRepository
                    .GetMany(category, int.MaxValue)
                    .OrderBy(x => x.CreatedAt) //The oldest will be served first
                    .ToList();

                var objectsToDelete =
                    (
                        //Select all duplicates and nuke them - entities are already tracked so no second round-trip needed
                        from update in updates
                        where !idsInQueue.Add(update.SourceId)
                        select update
                    )
                    .ToList();

                if (objectsToDelete.Any())
                {
                    _primitiveRepository.RemoveRange(objectsToDelete);

                    _primitiveRepository.Save();
                    transaction.Commit();
                    deleted += objectsToDelete.Count;
                }
            }

            return Task.FromResult(Result<string, OperationError>.Success($"Deleted {deleted} duplicated"));
        }
    }
}
