using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DataAccess;

namespace Core.BackgroundJobs.Model.ReadModels
{
    /// <summary>
    /// Based on updated dependencies, this job schedules new job
    /// </summary>
    public class ScheduleDataProcessingAgreementReadModelUpdates : IAsyncBackgroundJob
    {
        private readonly IPendingReadModelUpdateRepository _updateRepository;
        private readonly IDataProcessingAgreementReadModelRepository _readModelRepository;
        private readonly ITransactionManager _transactionManager;
        public string Id => StandardJobIds.ScheduleDataProcessingAgreementReadModelUpdates;
        private const int BatchSize = 500;

        public ScheduleDataProcessingAgreementReadModelUpdates(
            IPendingReadModelUpdateRepository updateRepository,
            IDataProcessingAgreementReadModelRepository readModelRepository,
            ITransactionManager transactionManager)
        {
            _updateRepository = updateRepository;
            _readModelRepository = readModelRepository;
            _transactionManager = transactionManager;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var updatesExecuted = 0;
            foreach (var userUpdate in _updateRepository.GetMany(PendingReadModelUpdateSourceCategory.DataProcessingAgreement_User, BatchSize).ToList())
            {
                if (token.IsCancellationRequested)
                    break;

                using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
                var updates = _readModelRepository.GetByUserId(userUpdate.Id).ToList().Select(dpa =>
                        PendingReadModelUpdate.Create(dpa,
                            PendingReadModelUpdateSourceCategory.DataProcessingAgreement))
                    .ToList();
                updates.ForEach(update => _updateRepository.AddIfNotPresent(update));
                _updateRepository.Delete(userUpdate);
                transaction.Commit();
                updatesExecuted++;
            }

            return Task.FromResult(Result<string, OperationError>.Success($"Completed {updatesExecuted} updates"));
        }
    }
}
