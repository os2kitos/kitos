using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class EmailRecipientWriteRequestDTO
    {
        /// <summary>
        /// Email of the recipient
        /// </summary>
        [Required]
        public required string Email { get; set; }
    }
}