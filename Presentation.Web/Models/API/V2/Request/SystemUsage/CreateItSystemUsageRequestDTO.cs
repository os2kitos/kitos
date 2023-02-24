using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class CreateItSystemUsageRequestDTO : BaseItSystemUsageWriteRequestDTO
    {
        /// <summary>
        /// Points to the main system which the usage will extend.
        /// Constraints:
        ///     - must be accessible to the authorized user
        ///     - must not already be in use in the organization
        ///     - system must be active iow. not in a disabled state
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid SystemUuid { get; set; }
        
        [Required]
        [NonEmptyGuid]
        public Guid OrganizationUuid { get; set; }

        public GeneralDataWriteRequestDTO General { get; set; }
        /// <summary>
        /// User defined external references.
        /// The external reference marked as "master reference" will be shown in overviews
        /// Constraint:
        ///     - If the list is not empty one (and only one) must be marked as the master reference.
        /// </summary>
        public IEnumerable<ExternalReferenceDataWriteRequestDTO> ExternalReferences { get; set; }
        /// <summary>
        /// Defines archiving related properties for this system usage
        /// </summary>
        public ArchivingCreationRequestDTO Archiving { get; set; }
    }
}