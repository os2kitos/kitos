using Core.BackgroundJobs.Model;

namespace Core.BackgroundJobs.Factory
{
    public interface IBackgroundJobFactory
    {
        IAsyncBackgroundJob CreateExternalReferenceCheck();
    }
}
