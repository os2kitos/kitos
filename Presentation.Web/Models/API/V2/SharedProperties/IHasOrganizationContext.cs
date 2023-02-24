using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Models.API.V2.SharedProperties
{
    public interface IHasOrganizationContext
    {
        [Required]
        public ShallowOrganizationResponseDTO OrganizationContext { get; set; }
    }
}
