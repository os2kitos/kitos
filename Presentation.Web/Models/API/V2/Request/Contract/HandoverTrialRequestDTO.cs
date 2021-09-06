using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class HandoverTrialRequestDTO
    {
        /// <summary>
        /// Mandatory handover trial type
        /// Constraints:
        ///     - If changed from current state, the option type must be available in the organization
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid HandoverTrialTypeUuid { get; set; }
        /// <summary>
        /// Constraints: Either Approved or Expected must be defined
        /// </summary>
        public DateTime? ExpectedAt { get; set; }
        /// <summary>
        /// Constraints: Either Approved or Expected must be defined
        /// </summary>
        public DateTime? ApprovedAt { get; set; }
    }
}