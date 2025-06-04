using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.BackgroundJobs.Factories;
using Core.BackgroundJobs.Model;
using Core.BackgroundJobs.Model.ExternalLinks;
using Core.BackgroundJobs.Model.Maintenance;
using Core.BackgroundJobs.Model.PublicMessages;
using Core.BackgroundJobs.Model.ReadModels;
using Infrastructure.Services.BackgroundJobs;
using Serilog;

namespace Core.BackgroundJobs.Services
{
    public class BackgroundJobLauncher : IBackgroundJobLauncher
    {
        private readonly ILogger _logger;
        private readonly CheckExternalLinksBackgroundJob _checkExternalLinksJob;
        private readonly RebuildDataProcessingRegistrationReadModelsBatchJob _rebuildDataProcessingRegistrationReadModels;
        private readonly ScheduleDataProcessingRegistrationReadModelUpdates _scheduleDataProcessingRegistrationReadModelUpdates;
        private readonly RebuildItSystemUsageOverviewReadModelsBatchJob _rebuildItSystemUsageOverviewReadModels;
        private readonly ScheduleItSystemUsageOverviewReadModelUpdates _scheduleItSystemUsageOverviewReadModelUpdates;
        private readonly IRebuildReadModelsJobFactory _rebuildReadModelsJobFactory;
        private readonly PurgeDuplicatePendingReadModelUpdates _purgeDuplicatePendingReadModelUpdates;
        private readonly ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveState _scheduleUpdatesForItSystemUsageReadModelsWhichChangesActive;
        private readonly PurgeOrphanedHangfireJobs _purgeOrphanedHangfireJobs;
        private readonly RebuildItContractOverviewReadModelsBatchJob _rebuildItContractOverviewReadModelsBatchJob;
        private readonly ScheduleItContractOverviewReadModelUpdates _scheduleItContractOverviewReadModelUpdates;
        private readonly ScheduleUpdatesForItContractOverviewReadModelsWhichChangesActiveState _contractOverviewReadModelsWhichChangesActiveState;
        private readonly ScheduleUpdatesForDataProcessingRegistrationOverviewReadModelsWhichChangesActiveState _scheduleUpdatesForDataProcessingRegistrationOverviewReadModelsWhichChangesActiveState;
        private readonly ScheduleFkOrgUpdatesBackgroundJob _scheduleFkOrgUpdatesBackgroundJob;
        private readonly CreateInitialPublicMessages _createInitialPublicMessages;
        private readonly CreateMainPublicMessage _createMainPublicMessage;

        public BackgroundJobLauncher(
            ILogger logger,
            CheckExternalLinksBackgroundJob checkExternalLinksJob,
            RebuildDataProcessingRegistrationReadModelsBatchJob rebuildDataProcessingRegistrationReadModels,
            ScheduleDataProcessingRegistrationReadModelUpdates scheduleDataProcessingRegistrationReadModelUpdates,
            RebuildItSystemUsageOverviewReadModelsBatchJob rebuildItSystemUsageOverviewReadModels,
            ScheduleItSystemUsageOverviewReadModelUpdates scheduleItSystemUsageOverviewReadModelUpdates,
            IRebuildReadModelsJobFactory rebuildReadModelsJobFactory,
            PurgeDuplicatePendingReadModelUpdates purgeDuplicatePendingReadModelUpdates,
            ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveState scheduleUpdatesForItSystemUsageReadModelsWhichChangesActive,
            PurgeOrphanedHangfireJobs purgeOrphanedHangfireJobs,
            RebuildItContractOverviewReadModelsBatchJob rebuildItContractOverviewReadModelsBatchJob,
            ScheduleItContractOverviewReadModelUpdates scheduleItContractOverviewReadModelUpdates,
            ScheduleUpdatesForItContractOverviewReadModelsWhichChangesActiveState contractOverviewReadModelsWhichChangesActiveState,
            ScheduleFkOrgUpdatesBackgroundJob scheduleFkOrgUpdatesBackgroundJob, 
            ScheduleUpdatesForDataProcessingRegistrationOverviewReadModelsWhichChangesActiveState scheduleUpdatesForDataProcessingRegistrationOverviewReadModelsWhichChangesActiveState,
            CreateInitialPublicMessages createInitialPublicMessages,
            CreateMainPublicMessage createMainPublicMessage)
        {
            _logger = logger;
            _checkExternalLinksJob = checkExternalLinksJob;
            _rebuildDataProcessingRegistrationReadModels = rebuildDataProcessingRegistrationReadModels;
            _scheduleDataProcessingRegistrationReadModelUpdates = scheduleDataProcessingRegistrationReadModelUpdates;
            _rebuildItSystemUsageOverviewReadModels = rebuildItSystemUsageOverviewReadModels;
            _scheduleItSystemUsageOverviewReadModelUpdates = scheduleItSystemUsageOverviewReadModelUpdates;
            _rebuildReadModelsJobFactory = rebuildReadModelsJobFactory;
            _purgeDuplicatePendingReadModelUpdates = purgeDuplicatePendingReadModelUpdates;
            _scheduleUpdatesForItSystemUsageReadModelsWhichChangesActive = scheduleUpdatesForItSystemUsageReadModelsWhichChangesActive;
            _purgeOrphanedHangfireJobs = purgeOrphanedHangfireJobs;
            _rebuildItContractOverviewReadModelsBatchJob = rebuildItContractOverviewReadModelsBatchJob;
            _scheduleItContractOverviewReadModelUpdates = scheduleItContractOverviewReadModelUpdates;
            _contractOverviewReadModelsWhichChangesActiveState = contractOverviewReadModelsWhichChangesActiveState;
            _scheduleFkOrgUpdatesBackgroundJob = scheduleFkOrgUpdatesBackgroundJob;
            _scheduleUpdatesForDataProcessingRegistrationOverviewReadModelsWhichChangesActiveState = scheduleUpdatesForDataProcessingRegistrationOverviewReadModelsWhichChangesActiveState;
            _createInitialPublicMessages = createInitialPublicMessages;
            _createMainPublicMessage = createMainPublicMessage;
        }

        public async Task LaunchUpdateItContractOverviewReadModels(CancellationToken token = default)
        {
            await Launch(_rebuildItContractOverviewReadModelsBatchJob, token);
        }

        public async Task LaunchUpdateStaleContractRmAsync(CancellationToken token = default)
        {
            await Launch(_contractOverviewReadModelsWhichChangesActiveState, token);
        }

        public async Task LaunchUpdateFkOrgSync(CancellationToken token = default)
        {
            await Launch(_scheduleFkOrgUpdatesBackgroundJob, token);
        }

        public async Task LaunchLinkCheckAsync(CancellationToken token = default)
        {
            await Launch(_checkExternalLinksJob, token);
        }

        public async Task LaunchUpdateDataProcessingRegistrationReadModels(CancellationToken token = default)
        {
            await Launch(_rebuildDataProcessingRegistrationReadModels, token);
        }

        public async Task LaunchScheduleDataProcessingRegistrationReadModelUpdates(CancellationToken token = default)
        {
            await Launch(_scheduleDataProcessingRegistrationReadModelUpdates, token);
        }

        public async Task LaunchUpdateStaleDataProcessingRegistrationReadModels(CancellationToken token = default)
        {
            await Launch(_scheduleUpdatesForDataProcessingRegistrationOverviewReadModelsWhichChangesActiveState, token);
        }

        public async Task LaunchScheduleItSystemUsageOverviewReadModelUpdates(CancellationToken token = default)
        {
            await Launch(_scheduleItSystemUsageOverviewReadModelUpdates, token);
        }

        public async Task LaunchScheduleItContractOverviewReadModelUpdates(CancellationToken token = default)
        {
            await Launch(_scheduleItContractOverviewReadModelUpdates, token);
        }

        public async Task LaunchUpdateItSystemUsageOverviewReadModels(CancellationToken token = default)
        {
            await Launch(_rebuildItSystemUsageOverviewReadModels, token);
        }

        public async Task LaunchFullReadModelRebuild(ReadModelRebuildScope scope, CancellationToken token)
        {
            var job = _rebuildReadModelsJobFactory.CreateRebuildJob(scope);
            await Launch(job, token);
        }

        public async Task LaunchPurgeDuplicatedReadModelUpdates(CancellationToken token)
        {
            await Launch(_purgeDuplicatePendingReadModelUpdates, token);
        }

        public async Task LaunchUpdateStaleSystemUsageRmAsync(CancellationToken token = default)
        {
            await Launch(_scheduleUpdatesForItSystemUsageReadModelsWhichChangesActive, token);
        }

        public async Task LaunchPurgeOrphanedHangfireJobs(CancellationToken token)
        {
            await Launch(_purgeOrphanedHangfireJobs, token);
        }

        public async Task LaunchCreatePublicMessagesTask(CancellationToken token = default)
        {
            await Launch(_createInitialPublicMessages, token);
        }

        public async Task LaunchCreateMainPublicMessageTask(CancellationToken token = default)
        {
            await Launch(_createMainPublicMessage, token);
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
            _logger.Verbose("'{jobName}' STARTING", jobId);
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
            _logger.Verbose("'{jobName}' SUCCEEDED with '{message}'", jobId, result.Value);
        }
    }
}
