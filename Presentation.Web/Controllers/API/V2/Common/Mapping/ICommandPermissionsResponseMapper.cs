using Core.ApplicationServices.Authorization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface ICommandPermissionsResponseMapper
    {
        CommandPermissionResponseDTO MapCommandPermission(CommandPermissionResult permission);
    }
}
