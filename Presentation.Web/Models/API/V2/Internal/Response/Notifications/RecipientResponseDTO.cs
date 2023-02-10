using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Notifications
{
    public class RecipientResponseDTO
    {
        public IEnumerable<EmailRecipientResponseDTO> EmailRecipients { get; set; }
        public IEnumerable<RoleRecipientResponseDTO> RoleRecipients { get; set; }
    }
}