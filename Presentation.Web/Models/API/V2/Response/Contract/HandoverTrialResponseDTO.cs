using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class HandoverTrialResponseDTO
    {
        /// <summary>
        /// Mandatory handover trial type
        [Required]
        [NonEmptyGuid]
        public IdentityNamePairResponseDTO HandoverTrialType { get; set; }
        public DateTime? ExpectedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}