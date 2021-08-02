using System;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request.Generic;
using Presentation.Web.Models.External.V2.Response;
using Presentation.Web.Models.External.V2.Types.SystemUsage;

namespace Presentation.Web.Models.External.V2.Request.SystemUsage
{
    public class BaseItSystemUsageWriteRequestDTO
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
        /// Determines if this system has been forced into valid state even if context properties would dictate otherwise (e.g. no longer in use)
        /// </summary>
        public bool EnforcedValid { get; set; }
        /// <summary>
        /// If specified, the system usage is valid from this date.
        /// </summary>
        public DateTime? ValidFrom { get; set; }
        /// <summary>
        /// If specified, the system usage is valid up until and including this date.
        /// </summary>
        public DateTime? ValidTo { get; set; }
        /// <summary>
        /// Defines the master contract for this system (many contracts can point to a system usage but only one can be the master contract)
        /// </summary>
        public Guid? MainContractUuid { get; set; }
        /// <summary>
        /// A collection of IT-System usage role option assignments
        /// </summary>
        public IEnumerable<RoleAssignmentRequestDTO> Roles { get; set; }
        /// <summary>
        /// Out of all of the using organization units, this one is responsible for the system within the organization.
        /// </summary>
        public Guid ResponsibleOrganizationUnitUuid { get; set; }
        /// <summary>
        /// Inherited KLE which have been removed locally
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> LocallyRemovedKLE { get; set; }
        /// <summary>
        /// KLE which has been added locally
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> LocallyAddedKLE { get; set; }
        /// <summary>
        /// IT-Projects associated with this system usage
        /// </summary>
        public IEnumerable<Guid> AssociatedProjectUuids { get; set; }
        /// <summary>
        /// User defined external references.
        /// The external reference marked as "master reference" will be shown in overviews and on the system front page in KITOS
        /// </summary>
        public IEnumerable<ExternalReferenceDTO> ExternalReferences { get; set; }
        /// <summary>
        /// Defines archiving related properties for this system usage
        /// </summary>
        public ArchivingWriteRequestDTO Archiving { get; set; }
        /// <summary>
        /// GDPR-specific registrations
        /// </summary>
        public GDPRWriteRequestDTO GDPR { get; set; }
    }
}