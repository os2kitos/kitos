using Presentation.Web.Infrastructure.Attributes;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.Generic.Roles
{
    public class BulkRoleAssignmentRequestDTO
    {
        /// <summary>
        /// UUID of the role option
        /// Constraints:
        ///     - Role must be available in the organization
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid RoleUuid { get; set; }

        /// <summary>
        /// UUIDs of users
        /// Constraints:
        ///     - every User must be a member of the organization
        /// </summary>
        [Required]
        public IEnumerable<Guid> UserUuids { get; set; }
    }
}