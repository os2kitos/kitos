using Presentation.Web.Infrastructure.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Types.Organization;

namespace Presentation.Web.Models.API.V2.Request.OrganizationUnit
{
    public class CreateOrganizationUnitRequestDTO
    {
        public string Name { get; set; }
        public OrganizationUnitOriginChoice Origin { get; set; }
        public Guid ParentUuid { get; set; }
    }
}