namespace Core.BackgroundJobs.Model
{
    public static class StandardJobIds
    {
        private const string NamePrefix = "kitos_background_job:";
        public static readonly string CheckExternalLinks = $"{NamePrefix}check-external-links";
        public static readonly string UpdateDataProcessingRegistrationReadModels = $"{NamePrefix}update-dpr-read-models";
        public static readonly string ScheduleDataProcessingRegistrationReadModelUpdates = $"{NamePrefix}schedule-dpr-read-model-updates";
        public static readonly string ScheduleItContractReadModelUpdates = $"{NamePrefix}schedule-contract-read-model-updates";
        public static readonly string ScheduleItSystemUsageOverviewReadModelUpdates = $"{NamePrefix}schedule-it-system-usage-overview-read-model-updates";
        public static readonly string UpdateItSystemUsageOverviewReadModels = $"{NamePrefix}update-it-system-usage-overview-read-models";
        public static readonly string UpdateItContractOverviewReadModels = $"{NamePrefix}update-it-contract-overview-read-models";
        public static readonly string RebuildDataProcessingReadModels = $"{NamePrefix}rebuild-dpr-read-models";
        public static readonly string RebuildItSystemUsageReadModels = $"{NamePrefix}rebuild-it-system-usage-read-models";
        public static readonly string RebuildItContractReadModels = $"{NamePrefix}rebuild-it-contract-read-models";
        public static readonly string PurgeDuplicatePendingReadModelUpdates = $"{NamePrefix}purge-duplicate-read-model-updates";
        public static readonly string ScheduleUpdatesForDataProcessingReadModelsWhichChangesActiveState = $"{NamePrefix}fix-stale-dpr-rms";
        public static readonly string ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveState = $"{NamePrefix}fix-stale-itsystem-usage-rms";
        public static readonly string ScheduleUpdatesForItContractOverviewReadModelsWhichChangesActiveState = $"{NamePrefix}fix-stale-itcontract-rms";
        public static readonly string ScheduleFkOrgUpdates = $"{NamePrefix}schedule-fk-org-updates";
        public static readonly string PurgeOrphanedHangfireJobs = $"{NamePrefix}purge-orphaned-hangfire-jobs";
        public static readonly string CreateInitialPublicMessages = $"{NamePrefix}create-initial-public-messages";
    }
}
