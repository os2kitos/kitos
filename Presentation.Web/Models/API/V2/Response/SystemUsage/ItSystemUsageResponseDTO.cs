using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;

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
        [Required]
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
        /// <summary>
        /// UTC timestamp of latest modification
        /// </summary>
        [Required]
        public DateTime LastModified { get; set; }
        /// <summary>
        /// Responsible for last modification
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }
        /// <summary>
        /// IT-System which this organizational usage is based on
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO SystemContext { get; set; }
        /// <summary>
        /// Organization in which the system usage has been created
        /// </summary>
        [Required]
        public ShallowOrganizationResponseDTO OrganizationContext { get; set; }
        
        [Required]
        public GeneralDataResponseDTO General { get; set; }
        /// <summary>
        /// A collection of IT-System usage role option assignments
        /// </summary>
        [Required]
        public IEnumerable<RoleAssignmentResponseDTO> Roles { get; set; }
        /// <summary>
        /// Defines the use of the system within the organization
        /// </summary>
        [Required]
        public OrganizationUsageResponseDTO OrganizationUsage { get; set; }
        /// <summary>
        /// Defines IT-System KLE deviations locally within an organization. All deviations are in the context of the inherited deviations which are found on the IT-System context
        /// </summary>
        [Required]
        public LocalKLEDeviationsResponseDTO LocalKLEDeviations { get; set; }
        /// <summary>
        /// User defined external references
        /// </summary>
        [Required]
        public IEnumerable<ExternalReferenceDataResponseDTO> ExternalReferences { get; set; }
        /// <summary>
        /// Archiving-specific registrations
        /// </summary>
        [Required]
        public ArchivingRegistrationsResponseDTO Archiving { get; set; }
        /// <summary>
        /// GDPR-specific registrations
        /// </summary>
        [Required]
        public GDPRRegistrationsResponseDTO GDPR { get; set; }
        /// <summary>
        /// Contains registered relations to other system usages within the organization
        /// </summary>
        [Required]
        public IEnumerable<OutgoingSystemRelationResponseDTO> OutgoingSystemRelations { get; set; }
    }
}