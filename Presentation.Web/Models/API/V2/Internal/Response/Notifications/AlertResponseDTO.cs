using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Models.API.V2.Internal.Response.Notifications
{
    public class AlertResponseDTO
    {
        [Required]
        public Guid Uuid { get; set; }

        [Required]
        public Guid EntityUuid { get; set; }

        public string Name { get; set; }

        public AlertType AlertType { get; set; }
        
        public string Message { get; set; }

        public DateTime? Created { get; set; }
    }
}