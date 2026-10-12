using System;

namespace Core.ApplicationServices.Model.GDPR.Write
{
    public class UpdatedDataProcessingRegistrationOversightDate
    {
        public DateTime? CompletedAt { get; set; }
        public string? Remark { get; set; }
        public string? OversightReportLink { get; set; }
        public string? OversightReportLinkName { get; set; }
        public Guid? OversightOptionUuid { get; set; }
    }
}