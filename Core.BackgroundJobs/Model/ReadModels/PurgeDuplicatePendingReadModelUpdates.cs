using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel.BackgroundJobs;
using Core.DomainServices;
using Core.DomainServices.Extensions;
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
                    .Select(x => new
                    {
                        Id = x.Id,
                        SourceId = x.SourceId
                    })
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
                    //Using optimized collection deletion. DeleteByKey first fetches the object from db and then marks as deleted. That is very slow since it will cause a lot of round-trips to the database in stead of one for select and one for delete
                    var objectsToDelete = _primitiveRepository.AsQueryable().ByIds(idsToDelete).ToList();
                    _primitiveRepository.RemoveRange(objectsToDelete);

                    _primitiveRepository.Save();
                    transaction.Commit();
                    deleted += idsToDelete.Count;
                }
            }

            return Task.FromResult(Result<string, OperationError>.Success($"Deleted {deleted} duplicated"));
        }
    }
}
