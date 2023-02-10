using System;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class RoleRecipientWriteRequestDTO
    {
        /// <summary>
        /// RoleUuid pointing to the Role to which the notification should be sent
        /// </summary>
        public Guid RoleUuid { get; set; }
    }
}