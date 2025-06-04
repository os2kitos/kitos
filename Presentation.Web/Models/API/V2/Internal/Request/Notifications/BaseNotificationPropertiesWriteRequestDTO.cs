using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class BaseNotificationPropertiesWriteRequestDTO
    {
        /// <summary>
        /// Subject of the Notification
        /// </summary>
        [Required]
        public string Subject { get; set; }

        public string Body { get; set; }

        /// <summary>
        /// Recipients to be mentioned in the CCs
        /// </summary>
        public RecipientWriteRequestDTO Ccs { get; set; }

        /// <summary>
        /// Recipients meant to receive the notification
        /// </summary>
        [Required]
        public RecipientWriteRequestDTO Receivers { get; set; }
    }
}