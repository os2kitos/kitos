using Core.BackgroundJobs.Model;
using Infrastructure.Services.BackgroundJobs;

namespace Core.BackgroundJobs.Factories
{
    public interface IRebuildReadModelsJobFactory
    {
        IAsyncBackgroundJob CreateRebuildJob(ReadModelRebuildScope scope);
    }
}
