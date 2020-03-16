namespace Infrastructure.Services.BackgroundJobs
{
    public interface IBackgroundJobManager
    {
        void TriggerLinkCheck();
    }
}
