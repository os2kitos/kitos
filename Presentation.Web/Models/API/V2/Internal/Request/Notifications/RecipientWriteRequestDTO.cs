using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class RecipientWriteRequestDTO
    {
        public IEnumerable<EmailRecipientWriteRequestDTO> EmailRecipients { get; set; }
        public IEnumerable<RoleRecipientWriteRequestDTO> RoleRecipients { get; set; }
    }
}