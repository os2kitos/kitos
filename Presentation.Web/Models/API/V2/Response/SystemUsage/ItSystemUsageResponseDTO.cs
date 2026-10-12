using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    /// <summary>
    /// NOTE: IT-System usages are registrations which extend those of a system within the context of a specific organization.
    /// IT-System usages have their own lifecycle and identity but the human readable name is inherited from the system context.
    /// </summary>
    public class ItSystemUsageResponseDTO : IHasUuidExternal, IHasLastModified, IHasOrganizationContext
    {
        /// <summary>
        /// UUID of the IT-System usage registration instance
        /// </summary>
        [Required]
        public Guid Uuid { get; set; }
        /// <summary>
        /// User who created the system usage
        /// </summary>
        public IdentityNamePairResponseDTO? CreatedBy { get; set; }
        /// <summary>
        /// UTC timestamp of latest modification
        /// </summary>
        [Required]
        public DateTime LastModified { get; set; }
        /// <summary>
        /// The time when this system usage was created
        /// </summary>
        public DateTime? CreatedAt { get; set; }
        /// <summary>
        /// Responsible for last modification
        /// </summary>
        public IdentityNamePairResponseDTO? LastModifiedBy { get; set; }
        /// <summary>
        /// IT-System which this organizational usage is based on
        /// </summary>
        [Required]
        public required IdentityNamePairResponseDTO SystemContext { get; set; }
        /// <summary>
        /// Organization in which the system usage has been created
        /// </summary>
        [Required]
        public ShallowOrganizationResponseDTO? OrganizationContext { get; set; }
        
        [Required]
        public required GeneralDataResponseDTO General { get; set; }
        /// <summary>
        /// A collection of IT-System usage role option assignments
        /// </summary>
        [Required]
        public required IEnumerable<RoleAssignmentResponseDTO> Roles { get; set; }
        /// <summary>
        /// Defines the use of the system within the organization
        /// </summary>
        [Required]
        public required OrganizationUsageResponseDTO OrganizationUsage { get; set; }
        /// <summary>
        /// Defines IT-System KLE deviations locally within an organization. All deviations are in the context of the inherited deviations which are found on the IT-System context
        /// </summary>
        [Required]
        public required LocalKLEDeviationsResponseDTO LocalKLEDeviations { get; set; }
        /// <summary>
        /// User defined external references
        /// </summary>
        [Required]
        public required IEnumerable<ExternalReferenceDataResponseDTO> ExternalReferences { get; set; }
        /// <summary>
        /// Archiving-specific registrations
        /// </summary>
        [Required]
        public required ArchivingRegistrationsResponseDTO Archiving { get; set; }
        /// <summary>
        /// GDPR-specific registrations
        /// </summary>
        [Required]
        public required GDPRRegistrationsResponseDTO GDPR { get; set; }
        /// <summary>
        /// Contains registered relations to other system usages within the organization
        /// </summary>
        [Required]
        public required IEnumerable<OutgoingSystemRelationResponseDTO> OutgoingSystemRelations { get; set; }
    }
}