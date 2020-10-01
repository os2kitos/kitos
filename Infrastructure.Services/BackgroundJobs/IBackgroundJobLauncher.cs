using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services.BackgroundJobs
{
    public interface IBackgroundJobLauncher
    {
        Task LaunchAdviceCleanupAsync();
        Task LaunchLinkCheckAsync(CancellationToken token = default);
        Task LaunchUpdateDataProcessingRegistrationReadModels(CancellationToken token = default);
        Task LaunchScheduleDataProcessingRegistrationReadUpdates(CancellationToken token = default);
    }
}
