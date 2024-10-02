using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response.User;
using System;

namespace Presentation.Web.Controllers.API.V2.Internal.Users.Mapping
{
    public interface IUserResponseModelMapper
    {
        UserResponseDTO ToUserResponseDTO(User user);
        UserWithIsPartOfCurrentOrgResponseDTO ToUserWithIsPartOfCurrentOrgResponseDTO(Guid organizationUuid, User user);
    }
}
