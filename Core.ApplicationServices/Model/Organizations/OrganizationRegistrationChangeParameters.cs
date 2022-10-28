using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationChangeParameters
    {
        public OrganizationRegistrationChangeParameters(
            IEnumerable<int> organizationUnitRights, 
            IEnumerable<int> itContractRegistrations, 
            IEnumerable<PaymentChangeParameters> paymentRegistrationDetails, 
            IEnumerable<int> responsibleSystems, 
            IEnumerable<int> relevantSystems)
        {
            OrganizationUnitRights = organizationUnitRights;
            ItContractRegistrations = itContractRegistrations;
            PaymentRegistrationDetails = paymentRegistrationDetails;
            ResponsibleSystems = responsibleSystems;
            RelevantSystems = relevantSystems;
        }

        public IEnumerable<int> OrganizationUnitRights { get; }
        public IEnumerable<int> ItContractRegistrations { get; }
        public IEnumerable<PaymentChangeParameters> PaymentRegistrationDetails { get; }
        public IEnumerable<int> ResponsibleSystems { get; }
        public IEnumerable<int> RelevantSystems { get; }
    }
}
