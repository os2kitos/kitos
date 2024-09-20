using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response.User;

namespace Presentation.Web.Controllers.API.V2.Internal.Users.Mapping
{
    public interface IUserResponseModelMapper
    {
        UserResponseDTO ToUserResponseDTO(User user);
    }
}
