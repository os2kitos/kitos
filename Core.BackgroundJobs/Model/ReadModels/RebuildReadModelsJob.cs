using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.BackgroundJobs;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class RebuildReadModelsJob : IAsyncBackgroundJob
    {
        private readonly Func<IEnumerable<int>> _getIds;
        private readonly PendingReadModelUpdateSourceCategory _sourceCategory;
        private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;
        public string Id { get; }

        public RebuildReadModelsJob(
            string id,
            Func<IEnumerable<int>> getIds,
            PendingReadModelUpdateSourceCategory sourceCategory,
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository)
        {
            _getIds = getIds;
            _sourceCategory = sourceCategory;
            _pendingReadModelUpdateRepository = pendingReadModelUpdateRepository;
            Id = id;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var modelUpdates = _getIds()
                .ToList()
                .Select(MapReadModelUpdate)
                .ToList();

            int currentBatchCount;
            const int maxBatchSize = 250;
            var total = 0;
            do
            {
                //EF does not deal well with too large outstanding transactions, so push it in batches
                var batch = modelUpdates.Skip(total).Take(maxBatchSize).ToList();
                _pendingReadModelUpdateRepository.AddMany(batch);
                currentBatchCount = batch.Count;
                total += currentBatchCount;
            } while (currentBatchCount == maxBatchSize && token.IsCancellationRequested == false);
            return Task.FromResult(Result<string, OperationError>.Success($"Ok - inserted {total} updates"));
        }

        private PendingReadModelUpdate MapReadModelUpdate(int id)
        {
            return PendingReadModelUpdate.Create(id, _sourceCategory);
        }
    }
}
