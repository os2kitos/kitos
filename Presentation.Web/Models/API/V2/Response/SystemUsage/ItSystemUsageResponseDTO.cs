using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    /// <summary>
    /// NOTE: IT-System usages are registrations which extend those of a system within the context of a specific organization.
    /// IT-System usages have their own lifecycle and identity but the human readable name is inherited from the system context.
    /// </summary>
    public class ItSystemUsageResponseDTO : IHasUuidExternal, IHasLastModified
    {
        /// <summary>
        /// UUID of the IT-System usage registration instance
        /// </summary>
        public Guid Uuid { get; set; }
        
        /// <summary>
        /// User who created the system usage
        /// </summary>
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
        /// <summary>
        /// Time of last modification
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Responsible for last modification
        /// </summary>
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


        public GeneralDataResponseDTO General { get; set; }
        /// <summary>
        /// A collection of IT-System usage role option assignments
        /// </summary>
        public IEnumerable<RoleAssignmentResponseDTO> Roles { get; set; }
        /// <summary>
        /// Defines the use of the system within the organization
        /// </summary>
        public OrganizationUsageResponseDTO OrganizationUsage { get; set; }
        /// <summary>
        /// Defines IT-System KLE deviations locally within an organization. All deviations are in the context of the inherited deviations which are found on the IT-System context
        /// </summary>
        public LocalKLEDeviationsResponseDTO LocalKLEDeviations { get; set; }
        /// <summary>
        /// User defined external references
        /// </summary>
        public IEnumerable<ExternalReferenceDataDTO> ExternalReferences { get; set; }
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
        public IEnumerable<SystemRelationResponseDTO> OutgoingSystemRelations { get; set; }
    }
}