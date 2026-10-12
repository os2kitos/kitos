using Core.ApplicationServices.Model.Users.Write;
using Presentation.Web.Models.API.V2.Request.User;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.Internal.Users.Mapping
{
    public interface IUserWriteModelMapper
    {
        CreateUserParameters FromPOST(CreateUserRequestDTO request);

        UpdateUserParameters FromPATCH(UpdateUserRequestDTO request);
    }
}
