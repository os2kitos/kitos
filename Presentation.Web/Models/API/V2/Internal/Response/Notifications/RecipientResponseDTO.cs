using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Notifications
{
    public class RecipientResponseDTO
    {
        /// <summary>
        /// List of recipient emails
        /// </summary>
        public IEnumerable<EmailRecipientResponseDTO> EmailRecipients { get; set; }
        /// <summary>
        /// List of recipient roles
        /// </summary>
        public IEnumerable<RoleRecipientResponseDTO> RoleRecipients { get; set; }
    }
}