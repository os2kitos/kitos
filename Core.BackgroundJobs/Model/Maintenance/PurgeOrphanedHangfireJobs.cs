using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.ScheduledJobs;
using Core.DomainServices.Repositories.Advice;

namespace Core.BackgroundJobs.Model.Maintenance
{
    public class PurgeOrphanedHangfireJobs : IAsyncBackgroundJob
    {
        private readonly IAdviceRepository _adviceRepository;
        private readonly IHangfireApi _hangfireApi;
        public string Id => StandardJobIds.PurgeOrphanedHangfireJobs;

        public PurgeOrphanedHangfireJobs(IAdviceRepository adviceRepository, IHangfireApi hangfireApi)
        {
            _adviceRepository = adviceRepository;
            _hangfireApi = hangfireApi;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var currentAdviceIdsFromDb = _adviceRepository.GetAllIds();
            var recurringJobsIdInfo = _hangfireApi.GetRecurringJobsIdInfo().ToList();
            var scheduledJobsIdInfo = _hangfireApi.GetScheduledJobsIdInfo().ToList();

            var adviceIdsTargetedByHangFire = GetAdviceIdsTargetedByHangFire(recurringJobsIdInfo, scheduledJobsIdInfo);

            var orphanedIds = adviceIdsTargetedByHangFire.Except(currentAdviceIdsFromDb).ToList();

            foreach (var adviceId in orphanedIds)
            {
                _hangfireApi.DeleteAdviceFromHangfire(adviceId, scheduledJobsIdInfo, recurringJobsIdInfo);
            }

            return Task.FromResult(Result<string, OperationError>.Success($"Purged jobs for {orphanedIds.Count} advices"));
        }

        private static IEnumerable<int> GetAdviceIdsTargetedByHangFire(IEnumerable<(int adviceId, string jobId)> recurringJobsIdInfo, IEnumerable<(int adviceId, string jobId)> scheduledJobsIdInfo)
        {
            return recurringJobsIdInfo
                .Concat(scheduledJobsIdInfo)
                .Select(ids => ids.adviceId)
                .Distinct()
                .ToList();
        }
    }
}
