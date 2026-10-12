using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.Shared;
using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Types.DataProcessing
{
    public class OversightDateDTO //: ModifyOversightDateDTO
    {
        public Guid Uuid { get; set; }

        /// <summary>
        /// Date of oversight completion
        /// </summary>
        [SupplierField]
        public DateTime CompletedAt { get; set; }
        /// <summary>
        /// Optional remark related to the oversight
        /// </summary>
        [SupplierField]
        public string? Remark { get; set; }

        public SimpleLinkDTO? OversightReportLink { get; set; }
        /// <summary>
        /// Optional UUID for the oversight option
        /// </summary>
        public IdentityNamePairResponseDTO? OversightOption { get; set; }
    }
}