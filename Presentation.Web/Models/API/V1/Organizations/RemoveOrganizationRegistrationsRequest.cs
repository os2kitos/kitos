using Presentation.Web.Models.API.V1.Users;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class RemoveOrganizationRegistrationsRequest
    {
        public RemoveOrganizationRegistrationsRequest()
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