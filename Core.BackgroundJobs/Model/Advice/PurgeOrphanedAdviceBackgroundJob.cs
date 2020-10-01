using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.Advice;
using Hangfire;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.BackgroundJobs.Model.Advice
{
    public class PurgeOrphanedAdviceBackgroundJob : IAsyncBackgroundJob
    {
        private readonly IAdviceRepository _adviceRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;

        public string Id => StandardJobIds.PurgeOrphanedAdvice;

        public PurgeOrphanedAdviceBackgroundJob(
            IAdviceRepository adviceRepository,
            ITransactionManager transactionManager,
            ILogger logger)
        {
            _adviceRepository = adviceRepository;
            _transactionManager = transactionManager;
            _logger = logger;
        }

        public Task<Result<string, OperationError>> ExecuteAsync()
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            var purgedAdviceCount = 0;

            var orphans = _adviceRepository.GetOrphans().ToList();
            if (orphans.Any())
            {
                _logger.Information("Purging {orphanCount} orphaned advices", orphans.Count);
                foreach (var orphan in orphans)
                {
                    try
                    {
                        _logger.Debug("Removing orphaned advice with id '{id}', job id '{jobId}',type '{relationType}', and parent id '{relationId}'", orphan.Id, orphan.JobId ?? string.Empty, orphan.Type, orphan.RelationId);
                        RecurringJob.RemoveIfExists(orphan.JobId ?? string.Empty);
                        _adviceRepository.Delete(orphan);
                        purgedAdviceCount++;
                        _logger.Debug("Removed advice with id '{adviceId}'", orphan.Id);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Failed while deleting advice with id: {adviceId}", orphan.Id);
                    }

                }
                _logger.Information("Purged {orphanCount} orphaned advices", purgedAdviceCount);
            }

            transaction.Commit();
            return Task.FromResult(Result<string, OperationError>.Success($"Purged {purgedAdviceCount} advices"));
        }
    }
}
