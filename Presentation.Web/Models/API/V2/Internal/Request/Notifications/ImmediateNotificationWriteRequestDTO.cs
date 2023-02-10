using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class ImmediateNotificationWriteRequestDTO
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public IdentityNamePairResponseDTO OwnerResource { get; set; }

        public RecipientWriteRequestDTO Ccs { get; set; }
        public RecipientWriteRequestDTO Receivers { get; set; }
    }
}