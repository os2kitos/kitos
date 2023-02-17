namespace Presentation.Web.Models.API.V2.Internal.Response.Notifications
{
    public class NotificationAccessRightsResponseDTO
    {
        public bool CanBeDeleted { get; set; }
        public bool CanBeDeactivated { get; set; }
        public bool CanBeModified { get; set; }
    }
}