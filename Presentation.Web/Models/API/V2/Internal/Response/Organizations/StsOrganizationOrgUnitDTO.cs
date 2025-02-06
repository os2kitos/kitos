using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class StsOrganizationOrgUnitDTO
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public IEnumerable<StsOrganizationOrgUnitDTO> Children { get; set; }
    }
}