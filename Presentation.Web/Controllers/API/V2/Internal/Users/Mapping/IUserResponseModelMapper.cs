using Core.Abstractions.Types;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response.User;
using System;

namespace Presentation.Web.Controllers.API.V2.Internal.Users.Mapping
{
    public interface IUserResponseModelMapper
    {
        Result<UserResponseDTO, OperationError> ToUserResponseDTO(Guid organizationUuid, User user);
        UserIsPartOfCurrentOrgResponseDTO ToUserWithIsPartOfCurrentOrgResponseDTO(Guid organizationUuid, User user);
    }
}
