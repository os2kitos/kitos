using System;
using System.ComponentModel.DataAnnotations;
using Core.DomainModel.Organization;

namespace Presentation.Web.Models.API.V2.Request.OrganizationUnit
{
    public class CreateOrganizationUnitRequestDTO : BaseOrganizationUnitRequestDTO
    {
        [Required]
        public Guid ParentUnitUuid { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public OrganizationUnitOrigin Origin { get; set; }
        public int EAN { get; set; }
        public int Id { get; set; }
}
}