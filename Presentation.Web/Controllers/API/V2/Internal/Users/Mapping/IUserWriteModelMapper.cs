using Core.ApplicationServices.Model.Users.Write;
using Presentation.Web.Models.API.V2.Request.User;

namespace Presentation.Web.Controllers.API.V2.Internal.Users.Mapping
{
    public interface IUserWriteModelMapper
    {
        CreateUserParameters FromPOST(CreateUserRequestDTO request);
    }
}
