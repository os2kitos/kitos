using System;

namespace Presentation.Web.Models.API.V2.Internal.Response.QA
{
    public class BrokenExternalReferencesReportStatusResponseDTO
    {
        public bool Available { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? BrokenLinksCount { get; set; }
    }
}