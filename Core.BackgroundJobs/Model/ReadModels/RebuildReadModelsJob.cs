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
            var candidateIds = _getIds()
                .Select(MapReadModelUpdate);
            _pendingReadModelUpdateRepository.AddMany(candidateIds);
            return Task.FromResult(Result<string, OperationError>.Success("Ok"));
        }

        private PendingReadModelUpdate MapReadModelUpdate(int id)
        {
            return PendingReadModelUpdate.Create(id, _sourceCategory);
        }
    }
}
