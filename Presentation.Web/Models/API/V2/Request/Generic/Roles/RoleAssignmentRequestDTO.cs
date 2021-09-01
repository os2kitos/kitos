using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.Generic.Roles
{
    public class RoleAssignmentRequestDTO
    {
        /// <summary>
        /// UUID of the role option
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid RoleUuid { get; set; }
        /// <summary>
        /// UUID of the user
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid UserUuid { get; set; }
    }
}