using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class StsOrganizationOrgUnitDTO
    {
        public Guid Uuid { get; set; }
        public required string Name { get; set; }
        public required IEnumerable<StsOrganizationOrgUnitDTO> Children { get; set; }
    }
}