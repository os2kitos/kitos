namespace Infrastructure.Services.BackgroundJobs
{
    public interface IBackgroundJobScheduler
    {
        void ScheduleLinkCheckForImmediateExecution();
    }
}
