using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.System;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class GeneralDataWriteRequestDTO
    {
        /// <summary>
        /// System Id assigned locally within the organization
        /// Max length: 200
        /// </summary>
        [MaxLength(ItSystemUsage.LongProperyMaxLength)]
        public string? LocalSystemId { get; set; }
        /// <summary>
        /// Call name used locally within the organization
        /// Max length: 100
        /// </summary>
        [MaxLength(ItSystemUsage.DefaultMaxLength)]
        public string? LocalCallName { get; set; }
        /// <summary>
        /// Optional classification of the registered data
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        [NonEmptyGuid]
        public Guid? DataClassificationUuid { get; set; }
        /// <summary>
        /// Notes relevant to the system usage within the organization
        /// </summary>
        public string? Notes { get; set; }
        /// <summary>
        /// Locally registered system version
        /// Max length: 100
        /// </summary>
        [MaxLength(ItSystemUsage.DefaultMaxLength)]
        public string? SystemVersion { get; set; }
        /// <summary>
        /// Interval which defines the number of expected users this system has within the organization
        /// NOTE: This property is currently constrained to accept only the following values [LowerBound,UpperBound]: {[0,9],[10,50],[50,100],[100,null]}
        /// We expect this to change in later releases of KITOS and for that reason we do not expose an enumeration field.
        /// </summary>
        public ExpectedUsersIntervalDTO? NumberOfExpectedUsers { get; set; }
        /// <summary>
        /// Validity of the system usage
        /// </summary>
        public ItSystemUsageValidityWriteRequestDTO? Validity { get; set; }

        /// <summary>
        /// Whether the system usage is known to include any kind of AI technology
        /// </summary>
        [SupplierField]
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
        public string? WebAccessibilityNotes { get; set; }

        /// <summary>
        /// Whether the system is considered socially critical.
        /// </summary>
        public YesNoDontKnowChoice? IsSociallyCritical { get; set; }
        /// <summary>
        /// Whether the system is considered business critical.
        /// </summary>
        public YesNoDontKnowChoice? IsBusinessCritical { get; set; }
        /// <summary>
        /// How critical this system usage is considered to be in general.
        /// </summary>
        [SupplierField]
        public Guid? SystemUsageCriticalityLevelUuid { get; set; }
        /// <summary>
        /// Documentation link for the criticality level assessment of this system usage.
        /// </summary>
        [SupplierField]
        public SimpleLinkDTO? CriticalityLevelDocumentation { get; set; }
        /// <summary>
        /// The general purpose of this system usage.
        /// </summary>
        [MaxLength(ItSystemUsage.LongProperyMaxLength)]
        public string? Purpose { get; set; }
        /// <summary>
        /// The technical system type classifications of this system usage (multi-choice).
        /// Constraint: If an update changes this field, all options identified must be currently available in the organization context
        /// </summary>
        public IEnumerable<Guid>? TechnicalSystemTypeUuids { get; set; }

        /// <summary>
        /// Which kind of hosting is used for this system usage.
        /// </summary>
        public HostingChoice? HostedAt { get; set; }

        /// <summary>
        /// The licensing and code models used for this system usage. Existing models will be replaced by the input data.
        /// </summary>
        public IEnumerable<LicensingAndCodeModelChoice>? LicensingAndCodeModels { get; set; }
    }
}