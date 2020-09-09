using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services.BackgroundJobs
{
    public interface IBackgroundJobLauncher
    {
        Task LaunchLinkCheckAsync(CancellationToken token = default);
        Task LaunchUpdateDataProcessingAgreementReadModels(CancellationToken token = default);
    }
}
