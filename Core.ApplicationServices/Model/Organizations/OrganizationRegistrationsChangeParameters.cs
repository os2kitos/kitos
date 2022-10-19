using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationsChangeParameters
    {
        public OrganizationRegistrationsChangeParameters()
        {
            RoleIds = new List<int>();
            InternalPaymentIds = new List<int>();
            ExternalPaymentIds = new List<int>();
            ContractWithRegistrationIds = new List<int>();
            RelevantSystems = new List<int>();
            ResponsibleSystemIds = new List<int>();
        }

        public IEnumerable<int> RoleIds { get; set; }
        public IEnumerable<int> InternalPaymentIds { get; set; }
        public IEnumerable<int> ExternalPaymentIds { get; set; }
        public IEnumerable<int> ContractWithRegistrationIds { get; set; }
        public IEnumerable<int> RelevantSystems { get; set; }
        public IEnumerable<int> ResponsibleSystemIds { get; set; }
    }
}
