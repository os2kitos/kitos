using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class ChangeOrganizationRegistrationsRequest
    {
        public ChangeOrganizationRegistrationsRequest()
        {

            Roles = new List<int>();
            InternalPayments = new List<int>();
            ExternalPayments = new List<int>();
            ContractRegistrations = new List<int>();
            RelevantSystems = new List<int>();
            ResponsibleSystems = new List<int>();
        }
        public IEnumerable<int> Roles { get; set; }
        public IEnumerable<int> InternalPayments{ get; set; }
        public IEnumerable<int> ExternalPayments{ get; set; }
        public IEnumerable<int> ContractRegistrations{ get; set; }
        public IEnumerable<int> ResponsibleSystems{ get; set; }
        public IEnumerable<int> RelevantSystems{ get; set; }
    }
}