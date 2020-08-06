using System.Collections.Generic;

namespace Presentation.Web.Models
{
    public class UserOrganizationsDTO
    {
        public ICollection<OrganizationDTO> Organizations { get; set; }
    }
}
