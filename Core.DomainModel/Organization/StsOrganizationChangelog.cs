using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.Organization
{
    public class StsOrganizationChangelog : Entity
    {
        public string ResponsibleEntityName { get; set; }
        public IEnumerable<StsOrganizationConsequenceLog> Consequences { get; set; }
    }
}
