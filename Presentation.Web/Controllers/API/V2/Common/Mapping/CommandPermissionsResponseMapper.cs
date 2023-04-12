using Core.ApplicationServices.Authorization;
using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public class CommandPermissionsResponseMapper : ICommandPermissionsResponseMapper
    {
        public CommandPermissionResponseDTO MapCommandPermission(CommandPermissionResult permission)
        {
            return new CommandPermissionResponseDTO
                {
                    Id = permission.Id,
                    CanExecute = permission.CanExecute
                };
        }
    }
}