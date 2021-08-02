using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.External.V2.SharedProperties;
using Presentation.Web.Models.External.V2.Types.Shared;
using Presentation.Web.Models.External.V2.Types.SystemUsage;

namespace Presentation.Web.Models.External.V2.Response.SystemUsage
{
    /// <summary>
    /// NOTE: IT-System usages are registrations which extend those of a system within the context of a specific organization.
    /// IT-System usages have their own lifecycle and identity but the human readable name is inherited from the system context.
    /// </summary>
    public class ItSystemUsageResponseDTO : IHasUuidExternal
    {
        /// <summary>
        /// UUID of the IT-System usage registration instance
        /// </summary>
        public Guid Uuid { get; set; }
        /// <summary>
        /// IT-System which this organizational usage is based on
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO SystemContext { get; set; }
        /// <summary>
        /// Organization in which the system usage has been created
        /// </summary>
        [Required]
        public ShallowOrganizationDTO OrganizationContext { get; set; }
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
        public ItSystemUsageValidityResponseDTO Validity { get; set; }
        /// <summary>
        /// Defines the master contract for this system (many contracts can point to a system usage but only one can be the master contract)
        /// </summary>
        public IdentityNamePairResponseDTO MainContract { get; set; }
        /// <summary>
        /// A collection of IT-System usage role option assignments
        /// </summary>
        public IEnumerable<RoleAssignmentResponseDTO> Roles { get; set; }
        /// <summary>
        /// A collection of organization units which have taken this system into use
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> OrganizationUnitsUsingThisSystem { get; set; }
        /// <summary>
        /// Out of all of the using organization units, this one is responsible for the system within the organization.
        /// </summary>
        public IdentityNamePairResponseDTO ResponsibleOrganizationUnit { get; set; }
        /// <summary>
        /// Defines IT-System KLE deviations locally within an organization. All deviations are in the context of the inherited deviations which are found on the IT-System context
        /// </summary>
        public LocalKLEDeviationsResponseDTO LocalKLEDeviations { get; set; }
        /// <summary>
        /// IT-Projects associated with this system usage
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> AssociatedProjects { get; set; }
        /// <summary>
        /// User defined external references
        /// </summary>
        public IEnumerable<ExternalReferenceDTO> ExternalReferences { get; set; }
        /// <summary>
        /// Archiving-specific registrations
        /// </summary>
        public ArchivingRegistrationsResponseDTO Archiving { get; set; }
        /// <summary>
        /// GDPR-specific registrations
        /// </summary>
        public GDPRRegistrationsResponseDTO GDPR { get; set; }
        /// <summary>
        /// Contains registered relations to other system usages within the organization
        /// </summary>
        public SystemRelationResponseDTO OutgoingSystemRelations { get; set; }
    }
}