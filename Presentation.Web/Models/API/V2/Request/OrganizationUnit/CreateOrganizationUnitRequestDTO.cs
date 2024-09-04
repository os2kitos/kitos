using System;
using Core.DomainModel.Organization;

namespace Presentation.Web.Models.API.V2.Request.OrganizationUnit
{
    public class CreateOrganizationUnitRequestDTO : BaseOrganizationUnitRequestDTO
    {
        public Guid ParentUnitUuid { get; set; }
        public string Name { get; set; }
        public int EAN { get; set; }
        public int Id { get; set; }
        public OrganizationUnitOrigin Origin { get; set; }
}
}