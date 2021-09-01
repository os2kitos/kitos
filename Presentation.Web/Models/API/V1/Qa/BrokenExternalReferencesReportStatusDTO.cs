using System;

namespace Presentation.Web.Models.API.V1.Qa
{
    public class BrokenExternalReferencesReportStatusDTO
    {
        public bool Available { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? BrokenLinksCount { get; set; }
    }
}