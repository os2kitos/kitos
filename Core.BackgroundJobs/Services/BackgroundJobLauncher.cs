using System;
using System.Threading;
using System.Threading.Tasks;
using Core.BackgroundJobs.Model;
using Core.BackgroundJobs.Model.Advice;
using Core.BackgroundJobs.Model.ExternalLinks;
using Core.BackgroundJobs.Model.ReadModels;
using Core.DomainModel.Result;
using Infrastructure.Services.BackgroundJobs;
using Serilog;

namespace Core.BackgroundJobs.Services
{
    public class BackgroundJobLauncher : IBackgroundJobLauncher
    {
        private readonly ILogger _logger;
        private readonly CheckExternalLinksBackgroundJob _checkExternalLinksJob;
        private readonly PurgeOrphanedAdviceBackgroundJob _purgeOrphanedAdviceBackgroundJob;
        private readonly RebuildDataProcessingRegistrationReadModelsBatchJob _rebuildDataProcessingRegistrationReadModels;
        private readonly ScheduleDataProcessingRegistrationReadModelUpdates _scheduleDataProcessingRegistrationReadModelUpdates;
        private readonly RebuildItSystemUsageOverviewReadModelsBatchJob _rebuildItSystemUsageOverviewReadModels;
        private readonly ScheduleItSystemUsageOverviewReadModelUpdates _scheduleItSystemUsageOverviewReadModelUpdates;

        public BackgroundJobLauncher(
            ILogger logger,
            CheckExternalLinksBackgroundJob checkExternalLinksJob,
            PurgeOrphanedAdviceBackgroundJob purgeOrphanedAdviceBackgroundJob,
            RebuildDataProcessingRegistrationReadModelsBatchJob rebuildDataProcessingRegistrationReadModels,
            ScheduleDataProcessingRegistrationReadModelUpdates scheduleDataProcessingRegistrationReadModelUpdates,
            RebuildItSystemUsageOverviewReadModelsBatchJob rebuildItSystemUsageOverviewReadModels,
            ScheduleItSystemUsageOverviewReadModelUpdates scheduleItSystemUsageOverviewReadModelUpdates)
        {
            _logger = logger;
            _checkExternalLinksJob = checkExternalLinksJob;
            _rebuildDataProcessingRegistrationReadModels = rebuildDataProcessingRegistrationReadModels;
            _scheduleDataProcessingRegistrationReadModelUpdates = scheduleDataProcessingRegistrationReadModelUpdates;
            _rebuildItSystemUsageOverviewReadModels = rebuildItSystemUsageOverviewReadModels;
            _scheduleItSystemUsageOverviewReadModelUpdates = scheduleItSystemUsageOverviewReadModelUpdates;
            _purgeOrphanedAdviceBackgroundJob = purgeOrphanedAdviceBackgroundJob;
        }

        public async Task LaunchLinkCheckAsync(CancellationToken token = default)
        {
            await Launch(_checkExternalLinksJob, token);
        }

        public async Task LaunchAdviceCleanupAsync(CancellationToken token = default)
        {
            await Launch(_purgeOrphanedAdviceBackgroundJob, token);
        }

        public async Task LaunchUpdateDataProcessingRegistrationReadModels(CancellationToken token = default)
        {
            await Launch(_rebuildDataProcessingRegistrationReadModels, token);
        }

        public async Task LaunchScheduleDataProcessingRegistrationReadModelUpdates(CancellationToken token = default)
        {
            await Launch(_scheduleDataProcessingRegistrationReadModelUpdates, token);
        }

        public async Task LaunchScheduleItSystemUsageOverviewReadModelUpdates(CancellationToken token = default)
        {
            await Launch(_scheduleItSystemUsageOverviewReadModelUpdates, token);
        }

        public async Task LaunchUpdateItSystemUsageOverviewReadModels(CancellationToken token = default)
        {
            await Launch(_rebuildItSystemUsageOverviewReadModels, token);
        }

        private async Task Launch(IAsyncBackgroundJob job, CancellationToken token = default)
        {
            var jobId = job.Id;

            LogJobStarted(jobId);
            try
            {
                var result = await job.ExecuteAsync(token);
                LogJobResult(jobId, result);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error during execution of job {jobId}", jobId);
                throw;
            }
        }

        private void LogJobStarted(string jobId)
        {
            _logger.Information("'{jobName}' STARTING", jobId);
        }

        private void LogJobResult(string jobId, Result<string, OperationError> result)
        {
            if (result.Ok)
            {
                LogJobSucceeded(jobId, result);
            }
            else
            {
                LogJobFailed(jobId, result);
            }
        }

        private void LogJobFailed(string jobId, Result<string, OperationError> result)
        {
            _logger.Error("'{jobName}' FAILED with '{error}'", jobId, result.Error);
        }

        private void LogJobSucceeded(string jobId, Result<string, OperationError> result)
        {
            _logger.Information("'{jobName}' SUCCEEDED with '{message}'", jobId, result.Value);
        }
    }
}
