using System;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class CreateDataProcessingRegistrationOversightDateDTO
    {
        public DateTime OversightDate { get; set; }
        public string? OversightRemark { get; set; }
        public SimpleLinkDTO? OversightReportLink { get; set; }
        public Guid? OversightOptionUuid { get; set; }
    }
}