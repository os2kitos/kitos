using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationsRoot
    {
        public OrganizationRegistrationsRoot()
        {
            Roles = new List<OrganizationRegistrationDetails>();
            InternalPayments = new List<OrganizationRegistrationContractPayment>();
            ExternalPayments = new List<OrganizationRegistrationContractPayment>();
            ContractRegistrations = new List<OrganizationRegistrationDetails>();
            RelevantSystemRegistrations = new List<OrganizationRegistrationDetailsWithObjectData>();
        }

        public IEnumerable<OrganizationRegistrationDetails> Roles { get; set; }
        public IEnumerable<OrganizationRegistrationContractPayment> InternalPayments { get; set; }
        public IEnumerable<OrganizationRegistrationContractPayment> ExternalPayments { get; set; }
        public IEnumerable<OrganizationRegistrationDetails> ContractRegistrations { get; set; }
        public IEnumerable<OrganizationRegistrationDetailsWithObjectData> RelevantSystemRegistrations { get; set; }
        public IEnumerable<OrganizationRegistrationDetails> ResponsibleSystemRegistrations { get; set; }
    }
}
