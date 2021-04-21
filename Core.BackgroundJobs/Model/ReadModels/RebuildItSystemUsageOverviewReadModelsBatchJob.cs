using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Result;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Types;
using Serilog;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class RebuildItSystemUsageOverviewReadModelsBatchJob : IAsyncBackgroundJob
    {
        private readonly ILogger _logger;
        private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;
        private readonly IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel> _updater;
        private readonly IItSystemUsageOverviewReadModelRepository _readModelRepository;
        private readonly IItSystemUsageRepository _sourceRepository;
        private readonly ITransactionManager _transactionManager;
        private const int BatchSize = 100;
        public string Id => StandardJobIds.UpdateItSystemUsageOverviewReadModels;

        public RebuildItSystemUsageOverviewReadModelsBatchJob(
            ILogger logger,
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository,
            IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel> updater,
            IItSystemUsageOverviewReadModelRepository readModelRepository,
            IItSystemUsageRepository sourceRepository,
            ITransactionManager transactionManager)
        {
            _logger = logger;
            _pendingReadModelUpdateRepository = pendingReadModelUpdateRepository;
            _updater = updater;
            _readModelRepository = readModelRepository;
            _sourceRepository = sourceRepository;
            _transactionManager = transactionManager;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var completedUpdates = 0;
            try
            {
                using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

                foreach (var pendingReadModelUpdate in _pendingReadModelUpdateRepository.GetMany(PendingReadModelUpdateSourceCategory.ItSystemUsage, BatchSize).ToList())
                {
                    if (token.IsCancellationRequested)
                        break;

                    _logger.Debug("Rebuilding read model for {category}:{sourceId}", pendingReadModelUpdate.Category, pendingReadModelUpdate.SourceId);
                    var source = _sourceRepository.GetSystemUsage(pendingReadModelUpdate.SourceId).FromNullable();
                    var readModelResult = _readModelRepository.GetBySourceId(pendingReadModelUpdate.SourceId);
                    if (source.HasValue)
                    {
                        var sourceValue = source.Value;

                        ApplyUpdate(readModelResult, sourceValue);
                    }
                    else
                    {
                        _logger.Information("Source object {category}:{sourceId} has been deleted since the update was scheduled. The update is ignored and readmodel is deleted if present.", pendingReadModelUpdate.Category, pendingReadModelUpdate.SourceId);
                        if (readModelResult.HasValue)
                        {
                            _logger.Information("Deleting orphaned read model with id {id}.", readModelResult.Value.Id);
                            _readModelRepository.Delete(readModelResult.Value);
                        }

                    }
                    completedUpdates++;
                    _pendingReadModelUpdateRepository.Delete(pendingReadModelUpdate);
                    _logger.Debug("Finished rebuilding read model for {category}:{sourceId}", pendingReadModelUpdate.Category, pendingReadModelUpdate.SourceId);
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error while performing rebuilds of {Id}");
                return Task.FromResult(Result<string, OperationError>.Failure(new OperationError("Error during rebuild", OperationFailure.UnknownError)));
            }
            return Task.FromResult(Result<string, OperationError>.Success($"Completed {completedUpdates} updates"));
        }

        private void ApplyUpdate(Maybe<ItSystemUsageOverviewReadModel> readModelResult, ItSystemUsage sourceValue)
        {
            var readModel = readModelResult.GetValueOrFallback(new ItSystemUsageOverviewReadModel());
            _updater.Apply(sourceValue, readModel);
            if (readModelResult.HasValue)
            {
                _readModelRepository.Update(readModel);
            }
            else
            {
                _readModelRepository.Add(readModel);
            }
        }
    }
}
