using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationRemoval
    {
        public IEnumerable<int> RoleIdsToRemove { get; set; }
        public IEnumerable<int> InternalPaymentIdsToRemove { get; set; }
        public IEnumerable<int> ExternalPaymentIdsToRemove { get; set; }
        public IEnumerable<int> ContractResponsibleUnitIdsToRemove { get; set; }
        public IEnumerable<int> RelevantSystemIdsToRemove { get; set; }
        public IEnumerable<int> ResponsibleSystemIdsToRemove { get; set; }
    }
}
