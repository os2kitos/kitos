using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public class OrganizationRegistrationDetails
    {
        public OrganizationRegistrationDetails(
            IEnumerable<OrganizationUnitRight> organizationUnitRights, 
            IEnumerable<ItContract.ItContract> itContractRegistrations, 
            IEnumerable<PaymentRegistrationDetails> paymentRegistrationDetails,
            IEnumerable<ItSystemUsage.ItSystemUsage> responsibleSystems, 
            IEnumerable<ItSystemUsage.ItSystemUsage> relevantSystems)
        {
            OrganizationUnitRights = organizationUnitRights;
            ItContractRegistrations = itContractRegistrations;
            PaymentRegistrationDetails = paymentRegistrationDetails;
            ResponsibleSystems = responsibleSystems;
            RelevantSystems = relevantSystems;
        }

        public IEnumerable<OrganizationUnitRight> OrganizationUnitRights { get; }
        public IEnumerable<ItContract.ItContract> ItContractRegistrations { get; }
        public IEnumerable<PaymentRegistrationDetails> PaymentRegistrationDetails { get; }
        public IEnumerable<ItSystemUsage.ItSystemUsage> ResponsibleSystems { get; }
        public IEnumerable<ItSystemUsage.ItSystemUsage> RelevantSystems { get; }
    }
}
