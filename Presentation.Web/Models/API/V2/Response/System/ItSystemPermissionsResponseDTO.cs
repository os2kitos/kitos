using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Types.System;

namespace Presentation.Web.Models.API.V2.Response.System
{
    public class ItSystemPermissionsResponseDTO : ResourcePermissionsResponseDTO
    {
        public IEnumerable<SystemDeletionConflict> DeletionConflicts { get; set; }
    }
}