using Core.ApplicationServices.Authorization;
using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface ICommandPermissionsResponseMapper
    {
        CommandPermissionResponseDTO MapCommandPermission(CommandPermissionResult permission);
    }
}
