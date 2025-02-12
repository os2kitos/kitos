using System;
using System.ComponentModel.DataAnnotations;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class GeneralDataWriteRequestDTO
    {
        /// <summary>
        /// System Id assigned locally within the organization
        /// Max length: 200
        /// </summary>
        [MaxLength(ItSystemUsage.LongProperyMaxLength)]
        public string LocalSystemId { get; set; }
        /// <summary>
        /// Call name used locally within the organization
        /// Max length: 100
        /// </summary>
        [MaxLength(ItSystemUsage.DefaultMaxLength)]
        public string LocalCallName { get; set; }
        /// <summary>
        /// Optional classification of the registered data
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        [NonEmptyGuid]
        public Guid? DataClassificationUuid { get; set; }
        /// <summary>
        /// Notes relevant to the system usage within the organization
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Locally registered system version
        /// Max length: 100
        /// </summary>
        [MaxLength(ItSystemUsage.DefaultMaxLength)]
        public string SystemVersion { get; set; }
        /// <summary>
        /// Interval which defines the number of expected users this system has within the organization
        /// NOTE: This property is currently constrained to accept only the following values [LowerBound,UpperBound]: {[0,9],[10,50],[50,100],[100,null]}
        /// We expect this to change in later releases of KITOS and for that reason we do not expose an enumeration field.
        /// </summary>
        public ExpectedUsersIntervalDTO NumberOfExpectedUsers { get; set; }
        /// <summary>
        /// Validity of the system usage
        /// </summary>
        public ItSystemUsageValidityWriteRequestDTO Validity { get; set; }

        /// <summary>
        /// Whether the system usage is known to include any kind of AI technology
        /// </summary>
        public YesNoUndecidedChoice? ContainsAITechnology { get; set; }
    }
}