using System;

namespace Presentation.Web.Models.External.V2.Types.SystemUsage
{
    public class JournalPeriodDTO
    {
        public string ArchiveId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Approved { get; set; }
    }
}