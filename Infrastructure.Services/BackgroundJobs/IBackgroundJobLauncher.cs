using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services.BackgroundJobs
{
    public interface IBackgroundJobLauncher
    {
        Task LaunchAdviceCleanupAsync(CancellationToken token = default);
        Task LaunchLinkCheckAsync(CancellationToken token = default);
        Task LaunchUpdateDataProcessingRegistrationReadModels(CancellationToken token = default);
        Task LaunchScheduleDataProcessingRegistrationReadModelUpdates(CancellationToken token = default);
        Task LaunchScheduleItSystemUsageOverviewReadModelUpdates(CancellationToken token = default);
        Task LaunchUpdateItSystemUsageOverviewReadModels(CancellationToken token = default);
        Task LaunchFullReadModelRebuild(ReadModelRebuildScope scope, CancellationToken token);
        Task LaunchPurgeDuplicatedReadModelUpdates(CancellationToken token);
    }
}
