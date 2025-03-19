using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class GeneralDataResponseDTO
    {
        /// <summary>
        /// System Id assigned locally within the organization
        /// </summary>
        public string LocalSystemId { get; set; }
        /// <summary>
        /// Call name used locally within the organization
        /// </summary>
        public string LocalCallName { get; set; }
        /// <summary>
        /// Optional classification of the registered data
        /// </summary>
        public IdentityNamePairResponseDTO DataClassification { get; set; }
        /// <summary>
        /// Notes relevant to the system usage within the organization
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Locally registered system version
        /// </summary>
        public string SystemVersion { get; set; }
        /// <summary>
        /// Interval which defines the number of expected users this system has within the organization
        /// </summary>
        public ExpectedUsersIntervalDTO NumberOfExpectedUsers { get; set; }
        /// <summary>
        /// Specifies the validity of this system usage
        /// </summary>
        [Required]
        public ItSystemUsageValidityResponseDTO Validity { get; set; }
        /// <summary>
        /// Defines the master contract for this system (many contracts can point to a system usage but only one can be the master contract)
        /// </summary>
        public IdentityNamePairResponseDTO MainContract { get; set; }

        public YesNoUndecidedChoice? ContainsAITechnology { get; set; }
        /// <summary>
        /// Defines if the system is web accessibility compliant
        /// </summary>
        public YesNoPartiallyChoice? WebAccessibilityCompliance { get; set; }
        /// <summary>
        /// Last time the supplier checked system web accessibility
        /// </summary>
        public DateTime? LastWebAccessibilityCheck { get; set; }
        /// <summary>
        /// Notes related to the web accessibility of the system
        /// </summary>
        public string WebAccessibilityNotes { get; set; }
    }
}