namespace Infrastructure.Services.BackgroundJobs
{
    public interface IBackgroundJobScheduler
    {
        void ScheduleLinkCheck();
    }
}
