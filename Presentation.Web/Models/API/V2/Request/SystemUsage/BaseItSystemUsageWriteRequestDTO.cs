using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public abstract class BaseItSystemUsageWriteRequestDTO
    {
        /// <summary>
        /// A collection of IT-System usage role option assignments
        /// Constraint: Duplicates are not allowed
        /// </summary>
        public IEnumerable<RoleAssignmentRequestDTO> Roles { get; set; }
        /// <summary>
        /// Contains organizational references for the system usage
        /// </summary>
        public OrganizationUsageWriteRequestDTO OrganizationUsage { get; set; }
        /// <summary>
        /// Added/removed KLE when compared to inherited KLE from the IT-System
        /// </summary>
        public LocalKLEDeviationsRequestDTO LocalKleDeviations { get; set; }
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