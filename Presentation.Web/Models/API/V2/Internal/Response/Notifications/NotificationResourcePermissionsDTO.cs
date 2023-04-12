using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Models.API.V2.Internal.Response.Notifications
{
    public class NotificationResourcePermissionsDTO : ResourcePermissionsResponseDTO
    {
        /// <summary>
        /// True when API client is allowed to DEACTIVATE the resource
        /// </summary>
        public bool Deactivate { get; set; }
    }
}