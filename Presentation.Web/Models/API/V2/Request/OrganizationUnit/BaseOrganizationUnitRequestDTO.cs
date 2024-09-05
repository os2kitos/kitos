using Presentation.Web.Models.API.V2.Types.Organization;
using System;

namespace Presentation.Web.Models.API.V2.Request.OrganizationUnit
{
    public class BaseOrganizationUnitRequestDTO
    {
        public string Name { get; set; }
        public OrganizationUnitOriginChoice Origin { get; set; }
        public Guid ParentUuid { get; set; }
        public long Ean { get; set; }
        public string LocalId { get; set; }
    }
}