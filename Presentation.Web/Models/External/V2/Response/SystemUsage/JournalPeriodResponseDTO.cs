using System;

namespace Presentation.Web.Models.External.V2.Response.SystemUsage
{
    public class JournalPeriodResponseDTO
    {
        public string ArchiveId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Approved { get; set; }
    }
}