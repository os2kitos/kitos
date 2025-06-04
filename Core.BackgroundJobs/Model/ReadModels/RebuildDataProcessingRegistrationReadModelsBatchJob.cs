using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DataAccess;

using Serilog;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class RebuildDataProcessingRegistrationReadModelsBatchJob : IAsyncBackgroundJob
    {
        private readonly ILogger _logger;
        private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;
        private readonly IReadModelUpdate<DataProcessingRegistration, DataProcessingRegistrationReadModel> _updater;
        private readonly IDataProcessingRegistrationReadModelRepository _readModelRepository;
        private readonly IDataProcessingRegistrationRepository _sourceRepository;
        private readonly ITransactionManager _transactionManager;
        private const int BatchSize = 25;
        public string Id => StandardJobIds.UpdateDataProcessingRegistrationReadModels;

        public RebuildDataProcessingRegistrationReadModelsBatchJob(
            ILogger logger,
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository,
            IReadModelUpdate<DataProcessingRegistration, DataProcessingRegistrationReadModel> updater,
            IDataProcessingRegistrationReadModelRepository readModelRepository,
            IDataProcessingRegistrationRepository sourceRepository,
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
                foreach (var pendingReadModelUpdate in _pendingReadModelUpdateRepository.GetMany(PendingReadModelUpdateSourceCategory.DataProcessingRegistration, BatchSize).ToList())
                {
                    if (token.IsCancellationRequested)
                        break;

                    using var transaction = _transactionManager.Begin();
                    _logger.Debug("Rebuilding read model for {category}:{sourceId}", pendingReadModelUpdate.Category, pendingReadModelUpdate.SourceId);
                    var source = _sourceRepository.GetById(pendingReadModelUpdate.SourceId);
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

        private void ApplyUpdate(Maybe<DataProcessingRegistrationReadModel> readModelResult, DataProcessingRegistration sourceValue)
        {
            var readModel = readModelResult.GetValueOrFallback(new DataProcessingRegistrationReadModel());
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
