using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationsChangeParameters
    {
        public IEnumerable<int> RoleIds { get; set; }
        public IEnumerable<int> InternalPaymentIds { get; set; }
        public IEnumerable<int> ExternalPaymentIds { get; set; }
        public IEnumerable<int> ContractWithRegistrationIds { get; set; }
        public IEnumerable<OrganizationRelevantSystem> RelevantSystems { get; set; }
        public IEnumerable<int> ResponsibleSystemIds { get; set; }
    }
}
