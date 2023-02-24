using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class RecipientWriteRequestDTO
    {
        /// <summary>
        /// Emails of the Recipients
        /// </summary>
        public IEnumerable<EmailRecipientWriteRequestDTO> EmailRecipients { get; set; }
        /// <summary>
        /// Uuids of the Recipient roles
        /// </summary>
        public IEnumerable<RoleRecipientWriteRequestDTO> RoleRecipients { get; set; }
    }
}