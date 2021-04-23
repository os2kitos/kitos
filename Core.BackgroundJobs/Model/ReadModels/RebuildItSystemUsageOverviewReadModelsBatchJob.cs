using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Result;
using Core.DomainServices;
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
        private readonly IGenericRepository<ItSystemUsageOverviewReadModel> _lowLevelReadModelRepository;
        private readonly IGenericRepository<PendingReadModelUpdate> _lowLevelPendingReadModelRepository;
        private readonly IDatabaseControl _databaseControl;
        private const int BatchSize = 25;
        public string Id => StandardJobIds.UpdateItSystemUsageOverviewReadModels;

        public RebuildItSystemUsageOverviewReadModelsBatchJob(
            ILogger logger,
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository,
            IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel> updater,
            IItSystemUsageOverviewReadModelRepository readModelRepository,
            IItSystemUsageRepository sourceRepository,
            ITransactionManager transactionManager,
            IGenericRepository<ItSystemUsageOverviewReadModel> lowLevelReadModelRepository, //NOTE: Using the primitive repositories on purpose since we want to reduce the amount of calls to SaveChanges as much as possible
            IGenericRepository<PendingReadModelUpdate> lowLevelPendingReadModelRepository,
            IDatabaseControl databaseControl)
        {
            _logger = logger;
            _pendingReadModelUpdateRepository = pendingReadModelUpdateRepository;
            _updater = updater;
            _readModelRepository = readModelRepository;
            _sourceRepository = sourceRepository;
            _transactionManager = transactionManager;
            _lowLevelReadModelRepository = lowLevelReadModelRepository;
            _lowLevelPendingReadModelRepository = lowLevelPendingReadModelRepository;
            _databaseControl = databaseControl;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var completedUpdates = 0;
            try
            {
                var pendingReadModelUpdates = _pendingReadModelUpdateRepository.GetMany(PendingReadModelUpdateSourceCategory.ItSystemUsage, BatchSize).ToList();
                foreach (var pendingReadModelUpdate in pendingReadModelUpdates)
                {
                    if (token.IsCancellationRequested)
                        break;
                    using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
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
                            _lowLevelReadModelRepository.Delete(readModelResult.Value);
                        }

                    }
                    completedUpdates++;
                    _lowLevelPendingReadModelRepository.Delete(pendingReadModelUpdate);
                    _databaseControl.SaveChanges();
                    transaction.Commit();
                    _logger.Debug("Finished rebuilding read model for {category}:{sourceId}", pendingReadModelUpdate.Category, pendingReadModelUpdate.SourceId);
                }
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
            if (readModelResult.HasValue == false) //Add it to the tracking graph
            {
                _lowLevelReadModelRepository.Insert(readModel);
            }
        }
    }
}
