using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services.BackgroundJobs
{
    public interface IBackgroundJobLauncher
    {
        Task LaunchLinkCheckAsync(CancellationToken token = default);
        Task LaunchUpdateDataProcessingRegistrationReadModels(CancellationToken token = default);
        Task LaunchScheduleDataProcessingRegistrationReadModelUpdates(CancellationToken token = default);
        Task LaunchUpdateStaleDataProcessingRegistrationReadModels(CancellationToken token = default);
        Task LaunchScheduleItSystemUsageOverviewReadModelUpdates(CancellationToken token = default);
        Task LaunchScheduleItContractOverviewReadModelUpdates(CancellationToken token = default);
        Task LaunchUpdateItSystemUsageOverviewReadModels(CancellationToken token = default);
        Task LaunchFullReadModelRebuild(ReadModelRebuildScope scope, CancellationToken token);
        Task LaunchPurgeDuplicatedReadModelUpdates(CancellationToken token);
        Task LaunchUpdateStaleSystemUsageRmAsync(CancellationToken token = default);
        Task LaunchPurgeOrphanedHangfireJobs(CancellationToken token);
        Task LaunchUpdateItContractOverviewReadModels(CancellationToken token = default);
        Task LaunchUpdateStaleContractRmAsync(CancellationToken token = default);
        Task LaunchUpdateFkOrgSync(CancellationToken token = default);
        Task LaunchCreatePublicMessagesTask(CancellationToken token = default);
    }
}
