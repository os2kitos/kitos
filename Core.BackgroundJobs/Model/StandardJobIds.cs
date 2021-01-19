namespace Core.BackgroundJobs.Model
{
    public static class StandardJobIds
    {
        private const string NamePrefix = "kitos_background_job:";
        public static readonly string CheckExternalLinks = $"{NamePrefix}check-external-links";
        public static readonly string PurgeOrphanedAdvice = $"{NamePrefix}purge-orphaned-advice";
        public static readonly string UpdateDataProcessingRegistrationReadModels = $"{NamePrefix}update-dpr-read-models";
        public static readonly string ScheduleDataProcessingRegistrationReadModelUpdates = $"{NamePrefix}schedule-dpr-read-model-updates";
    }
}
