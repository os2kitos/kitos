using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.Interface;

namespace Presentation.Web.Models.API.V2.Response.Interface
{
    public class ItInterfacePermissionsResponseDTO : ResourcePermissionsResponseDTO, IHasDeletionConflicts<ItInterfaceDeletionConflict>
    {
        public required IEnumerable<ItInterfaceDeletionConflict> DeletionConflicts { get; set; }
    }
}