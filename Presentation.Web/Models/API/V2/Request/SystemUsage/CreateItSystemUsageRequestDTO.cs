using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

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
    }
}