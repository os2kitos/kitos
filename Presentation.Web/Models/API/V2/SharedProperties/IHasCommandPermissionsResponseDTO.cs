using Presentation.Web.Models.API.V2.Response.Shared;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.SharedProperties
{
    public interface IHasCommandPermissionsResponseDTO
    {
        IEnumerable<CommandPermissionResponseDTO> Commands { get; set; }
    }
}
