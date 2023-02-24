using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class ImmediateNotificationWriteRequestDTO : IHasBaseWriteProperties
    {
        /// <summary>
        /// Common Notification properties
        /// </summary>
        [Required] 
        public BaseNotificationPropertiesWriteRequestDTO BaseProperties { get; set; }
    }
}