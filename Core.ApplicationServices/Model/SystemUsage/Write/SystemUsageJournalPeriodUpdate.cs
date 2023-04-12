using System;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class SystemUsageJournalPeriodUpdate : SystemUsageJournalPeriodProperties
    {
        public Guid? Uuid { get; set; }
    }
}
