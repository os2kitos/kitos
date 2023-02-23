using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class RoleRecipientWriteRequestDTO
    {
        /// <summary>
        /// RoleUuid pointing to the Role to which the notification should be sent
        /// </summary>
        [Required]
        public Guid RoleUuid { get; set; }
    }
}