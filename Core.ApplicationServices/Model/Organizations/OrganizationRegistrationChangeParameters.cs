using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationChangeParameters
    {
        public OrganizationRegistrationChangeParameters()
        {
            OrganizationUnitRights = new List<int>();
            ItContractRegistrations = new List<int>();
            PaymentRegistrationDetails = new List<PaymentChangeParameters>();
            ResponsibleSystems = new List<int>();
            RelevantSystems = new List<int>();
        }

        public IEnumerable<int> OrganizationUnitRights { get; set; }
        public IEnumerable<int> ItContractRegistrations { get; set; }
        public IEnumerable<PaymentChangeParameters> PaymentRegistrationDetails { get; set; }
        public IEnumerable<int> ResponsibleSystems { get; set; }
        public IEnumerable<int> RelevantSystems { get; set; }
    }
}
