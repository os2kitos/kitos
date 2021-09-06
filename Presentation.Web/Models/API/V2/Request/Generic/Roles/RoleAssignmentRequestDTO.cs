using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.Generic.Roles
{
    public class RoleAssignmentRequestDTO
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
        /// UUID of the user
        /// Constraints:
        ///     - User must be a member of the organization
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid UserUuid { get; set; }
    }
}