using Core.ApplicationServices.Model.Messages;
using Presentation.Web.Models.API.V2.Internal.Request;

namespace Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping
{
    public interface IPublicMessagesWriteModelMapper
    {
        WritePublicMessagesParams FromPOST(PublicMessageRequestDTO request);
        WritePublicMessagesParams FromPATCH(PublicMessageRequestDTO request);
    }
}
