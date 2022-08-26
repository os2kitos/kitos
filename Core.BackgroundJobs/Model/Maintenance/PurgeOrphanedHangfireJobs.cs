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
            var adviceIds = _adviceRepository.GetAllIds();
            var adviceIdsTargetedByHangFire = GetAdviceIdsTargetedByHangFire();
            var orphanedIds = adviceIdsTargetedByHangFire.Except(adviceIds).ToList();

            foreach (var adviceId in orphanedIds)
            {
                _hangfireApi.DeleteAdviceFromHangfire(adviceId);
            }

            return Task.FromResult(Result<string, OperationError>.Success($"Purged jobs for {orphanedIds.Count} advices"));
        }

        private ISet<int> GetAdviceIdsTargetedByHangFire()
        {
            return _hangfireApi
                .GetRecurringJobsIdInfo()
                .Concat(_hangfireApi.GetScheduledJobsIdInfo())
                .Select(x => x.adviceId)
                .ToHashSet();
        }
    }
}
