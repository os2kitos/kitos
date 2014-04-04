using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.MVC4.Models
{
    public class OrgUnitDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OrganizationId { get; set; }
        public int ParentId { get; set; }
        public List<OrgUnitDTO> Children { get; set; } 
    }
}