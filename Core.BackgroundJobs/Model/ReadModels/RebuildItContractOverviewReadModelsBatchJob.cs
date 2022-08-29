using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.Contract;
using Infrastructure.Services.DataAccess;

using Serilog;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class RebuildItContractOverviewReadModelsBatchJob : IAsyncBackgroundJob
    {
        private readonly ILogger _logger;
        private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;
        private readonly IReadModelUpdate<ItContract, ItContractOverviewReadModel> _updater;
        private readonly IItContractOverviewReadModelRepository _readModelRepository;
        private readonly IItContractRepository _sourceRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IGenericRepository<ItContractOverviewReadModel> _lowLevelReadModelRepository;
        private readonly IGenericRepository<PendingReadModelUpdate> _lowLevelPendingReadModelRepository;
        private readonly IDatabaseControl _databaseControl;
        private const int BatchSize = 25;
        public string Id => StandardJobIds.UpdateItContractOverviewReadModels;

        public RebuildItContractOverviewReadModelsBatchJob(
            ILogger logger,
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository,
            IReadModelUpdate<ItContract, ItContractOverviewReadModel> updater,
            IItContractOverviewReadModelRepository readModelRepository,
            IItContractRepository sourceRepository,
            ITransactionManager transactionManager,
            IGenericRepository<ItContractOverviewReadModel> lowLevelReadModelRepository, //NOTE: Using the primitive repositories on purpose since we want to reduce the amount of calls to SaveChanges as much as possible
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
                var pendingReadModelUpdates = _pendingReadModelUpdateRepository.GetMany(PendingReadModelUpdateSourceCategory.ItContract, BatchSize).ToList();
                foreach (var pendingReadModelUpdate in pendingReadModelUpdates)
                {
                    if (token.IsCancellationRequested)
                        break;
                    using var transaction = _transactionManager.Begin();
                    _logger.Debug("Rebuilding read model for {category}:{sourceId}", pendingReadModelUpdate.Category, pendingReadModelUpdate.SourceId);
                    var source = _sourceRepository.GetById(pendingReadModelUpdate.SourceId).FromNullable();
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

        private void ApplyUpdate(Maybe<ItContractOverviewReadModel> readModelResult, ItContract sourceValue)
        {
            var readModel = readModelResult.GetValueOrFallback(new ItContractOverviewReadModel());
            _updater.Apply(sourceValue, readModel);
            if (readModelResult.HasValue == false) //Add it to the tracking graph
            {
                _lowLevelReadModelRepository.Insert(readModel);
            }
        }
    }
}
