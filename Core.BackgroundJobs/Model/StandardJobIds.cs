namespace Core.BackgroundJobs.Model
{
    public static class StandardJobIds
    {
        private const string NamePrefix = "kitos_background_job:";
        public static readonly string CheckExternalLinks = $"{NamePrefix}check-external-links";
    }
}
