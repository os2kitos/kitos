using Presentation.Web.Models.API.V2.Response.Shared;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage
{
    public class ItSystemUsageMigrationPermissionsResponseDTO: IHasCommandPermissionsResponseDTO
    {
        public ItSystemUsageMigrationPermissionsResponseDTO(IEnumerable<CommandPermissionResponseDTO> commands)
        {
            Commands = commands;
        }

        public IEnumerable<CommandPermissionResponseDTO> Commands { get; }
    }
}