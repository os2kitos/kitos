namespace Core.BackgroundJobs.Model
{
    public static class StandardJobIds
    {
        private const string NamePrefix = "kitos_background_job:";
        public static readonly string CheckExternalLinks = $"{NamePrefix}check-external-links";
        public static readonly string UpdateDataProcessingRegistrationReadModels = $"{NamePrefix}update-dpa-read-models";
        public static readonly string ScheduleDataProcessingRegistrationReadModelUpdates = $"{NamePrefix}schedule-dp-read-model-updates";
    }
}
