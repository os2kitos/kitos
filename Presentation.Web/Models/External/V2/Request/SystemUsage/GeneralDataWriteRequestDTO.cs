using System;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Types.SystemUsage;

namespace Presentation.Web.Models.External.V2.Request.SystemUsage
{
    public class GeneralDataWriteRequestDTO
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
        [NonEmptyGuid]
        public Guid? DataClassificationUuid { get; set; }
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
        /// NOTE: This property is currently constrained to accept only the following values [LowerBound,UpperBound]: {[0,10],[10,50],[50,100],[100,null]}
        /// We expect this to change in later releases of KITOS and for that reason we do not expose an enumeration field.
        /// </summary>
        public ExpectedUsersIntervalDTO NumberOfExpectedUsers { get; set; }
        /// <summary>
        /// Validity of the system usage
        /// </summary>
        public ValidityWriteRequestDTO Validity { get; set; }
        /// <summary>
        /// Defines the master contract for this system (many contracts can point to a system usage but only one can be the master contract)
        /// </summary>
        [NonEmptyGuid]
        public Guid? MainContractUuid { get; set; }
        /// <summary>
        /// UUIds of Organization units using this system
        /// </summary>
        public IEnumerable<Guid> UsingOrganizationUnitUuids { get; set; }
        /// <summary>
        /// Out of all of the using organization units, this one is responsible for the system within the organization.
        /// </summary>
        [NonEmptyGuid]
        public Guid? ResponsibleOrganizationUnitUuid { get; set; }
        /// <summary>
        /// IT-Projects associated with this system usage
        /// </summary>
        public IEnumerable<Guid> AssociatedProjectUuids { get; set; }
    }
}