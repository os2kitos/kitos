using System.Threading.Tasks;

namespace Core.BackgroundJobs.Services
{
    public interface IBackgroundJobLauncher
    {
        Task LaunchLinkCheckAsync();
    }
}
