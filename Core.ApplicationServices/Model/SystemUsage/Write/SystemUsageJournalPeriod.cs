using System;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class SystemUsageJournalPeriod
    {
        public string ArchiveId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Approved { get; set; }
    }
}
