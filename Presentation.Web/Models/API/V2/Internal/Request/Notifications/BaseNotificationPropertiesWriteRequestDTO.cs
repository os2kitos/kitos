using System.ComponentModel.DataAnnotations;
using System;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class BaseNotificationPropertiesWriteRequestDTO
    {
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }

        public RecipientWriteRequestDTO Ccs { get; set; }
        public RecipientWriteRequestDTO Receivers { get; set; }
    }
}