using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Result;
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

        public async Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            foreach (var category in Enum.GetValues(typeof(PendingReadModelUpdateSourceCategory)).Cast<PendingReadModelUpdateSourceCategory>().ToList())
            {
                using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
                var idsInQueue = new HashSet<int>();

                var updates = _pendingReadModelUpdateRepository
                    .GetMany(category, int.MaxValue)
                    .OrderBy(x => x.CreatedAt) //The oldest will be served first
                    .Select(x => new { Id = x.Id, SourceId = x.SourceId })
                    .ToList();

                var idsToDelete =
                    (
                        //Select ids of all duplicates and nuke them
                        from update in updates
                        where !idsInQueue.Add(update.SourceId)
                        select update.Id
                    )
                    .ToList();

                if (idsToDelete.Any())
                {
                    idsToDelete.ForEach(id => _primitiveRepository.DeleteByKey(id));
                    _primitiveRepository.Save();
                    transaction.Commit();
                }
            }

            return "Ok";
        }
    }
}
