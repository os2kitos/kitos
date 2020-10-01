using System.Threading.Tasks;

namespace Infrastructure.Services.BackgroundJobs
{
    public interface IBackgroundJobLauncher
    {
        Task LaunchLinkCheckAsync();
        Task LaunchAdviceCleanupAsync();
    }
}
