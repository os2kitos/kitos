using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.Organization
{
   public class DataProtectionAdvisor: Entity, IOrganizationModule
    {
        public string Name { get; set; }
        public string Cvr { get; set; }
        public string Phone { get; set; }
        public string Adress { get; set; }
        public string Email { get; set; }
        public int? OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
    }
}
