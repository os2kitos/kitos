using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.System;

namespace Presentation.Web.Models.API.V2.Response.System
{
    public class ItSystemPermissionsResponseDTO : ResourcePermissionsResponseDTO, IHasDeletionConflicts<SystemDeletionConflict>
    {
        public IEnumerable<SystemDeletionConflict> DeletionConflicts { get; set; }
        public bool ModifyVisibility { get; set; }
    }
}