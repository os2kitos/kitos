using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1
{
    public class UserOrganizationsDTO
    {
        public ICollection<OrganizationDTO> Organizations { get; set; }
    }
}
