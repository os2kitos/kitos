using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationDetails
    {
        public OrganizationRegistrationDetails()
        {
            OrganizationUnitRights = new List<OrganizationUnitRight>();
            ItContractRegistrations = new List<ItContract>();
            PaymentRegistrationDetails = new List<PaymentRegistrationDetails>();
            ResponsibleSystems = new List<ItSystemUsage>();
            RelevantSystems = new List<ItSystemUsage>();
        }

        public IEnumerable<OrganizationUnitRight> OrganizationUnitRights { get; set; }
        public IEnumerable<ItContract> ItContractRegistrations { get; set; }
        public IEnumerable<PaymentRegistrationDetails> PaymentRegistrationDetails { get; set; }
        public IEnumerable<ItSystemUsage> ResponsibleSystems { get; set; }
        public IEnumerable<ItSystemUsage> RelevantSystems { get; set; }
    }
}
