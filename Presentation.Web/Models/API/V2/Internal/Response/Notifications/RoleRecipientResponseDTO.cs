using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Response.Notifications
{
    public class RoleRecipientResponseDTO
    {
        /// <summary>
        /// Role to which the notification should be sent
        /// </summary>
        public IdentityNamePairResponseDTO Role { get; set; }
    }
}