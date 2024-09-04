using Presentation.Web.Models.API.V2.Types.Organization;
using System;

namespace Presentation.Web.Models.API.V2.Request.OrganizationUnit
{
    public class BaseOrganizationUnitRequestDTO
    {
        public string Name { get; set; }
        public OrganizationUnitOriginChoice Origin { get; set; }
        public Guid ParentUuid { get; set; }
        public int EAN { get; set; }
        public int Id { get; set; }
    }
}