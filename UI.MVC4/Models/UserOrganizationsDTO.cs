using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.MVC4.Models
{
    public class UserOrganizationsDTO
    {
        public ICollection<OrganizationDTO> Organizations { get; set; }
        public int DefaultOrganizationId { get; set; }
    }
}