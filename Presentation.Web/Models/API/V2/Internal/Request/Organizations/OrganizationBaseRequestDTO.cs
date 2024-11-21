using System;
using System.ComponentModel.DataAnnotations;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;

namespace Presentation.Web.Models.API.V2.Internal.Request.Organizations
{
    public class OrganizationBaseRequestDTO
    {
        public string Name { get; set; }
        public OrganizationType Type { get; set; }
        [MaxLength(10)]
        public string Cvr {  get; set; }
        public Guid? ForeignCountryCodeUuid { get; set; }
    }
}