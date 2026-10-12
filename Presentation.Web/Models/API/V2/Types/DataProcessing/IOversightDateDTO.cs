using Presentation.Web.Models.API.V2.Types.Shared;
using System;

namespace Presentation.Web.Models.API.V2.Types.DataProcessing
{
    public interface IOversightDateDTO
    {
        public DateTime CompletedAt { get; set; }
        public string? Remark { get; set; }

        public SimpleLinkDTO? OversightReportLink { get; set; }
        public Guid? OversightOptionUuid { get; set; }
    }
}
