using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class RebuildDataProcessingAgreementReadModelsBatchJob : IAsyncBackgroundJob
    {
        private readonly ILogger _logger;
        private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;
        private readonly IReadModelUpdate<DataProcessingAgreement, DataProcessingAgreementReadModel> _updater;
        private readonly IDataProcessingAgreementReadModelRepository _readModelRepository;
        private readonly IDataProcessingAgreementRepository _sourceRepository;
        private readonly ITransactionManager _transactionManager;
        private const int BatchSize = 500;
        public string Id => StandardJobIds.UpdateDataProcessingAgreementReadModels;

        public RebuildDataProcessingAgreementReadModelsBatchJob(
            ILogger logger,
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository,
            IReadModelUpdate<DataProcessingAgreement, DataProcessingAgreementReadModel> updater,
            IDataProcessingAgreementReadModelRepository readModelRepository,
            IDataProcessingAgreementRepository sourceRepository,
            ITransactionManager transactionManager)
        {
            _logger = logger;
            _pendingReadModelUpdateRepository = pendingReadModelUpdateRepository;
            _updater = updater;
            _readModelRepository = readModelRepository;
            _sourceRepository = sourceRepository;
            _transactionManager = transactionManager;
        }

        public Task<Result<string, OperationError>> ExecuteAsync()
        {
            var completedUpdates = 0;
            try
            {
                foreach (var pendingReadModelUpdate in _pendingReadModelUpdateRepository.GetMany(PendingReadModelUpdateSourceCategory.DataProcessingAgreement, BatchSize).ToList())
                {
                    using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
                    _logger.Debug("Rebuilding read model for {category}:{sourceId}", pendingReadModelUpdate.Category, pendingReadModelUpdate.SourceId);
                    var source = _sourceRepository.GetById(pendingReadModelUpdate.SourceId);
                    if (source.HasValue)
                    {
                        var bySourceId = _readModelRepository.GetBySourceId(pendingReadModelUpdate.SourceId);

                        var readModel = bySourceId.GetValueOrFallback(new DataProcessingAgreementReadModel());
                        _updater.Apply(source.Value, readModel);
                        if (bySourceId.HasValue)
                        {
                            _readModelRepository.Update(readModel);
                        }
                        else
                        {
                            _readModelRepository.Add(readModel);
                        }
                    }
                    else
                    {
                        _logger.Information("Source object {category}:{sourceId} has been deleted since the update was scheduled. The update is ignored.", pendingReadModelUpdate.Category, pendingReadModelUpdate.SourceId);
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
    }
}
