using Presentation.Web.Models.API.V1.Users;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class RemoveOrganizationRegistrationsRequest
    {
        [Required]
        public IEnumerable<int> Roles { get; set; }
        public IEnumerable<int> InternalPayments{ get; set; }
        public IEnumerable<int> ExternalPayments{ get; set; }
        public IEnumerable<int> ContractRegistrations{ get; set; }
        public IEnumerable<int> ResponsibleSystems{ get; set; }
        public IEnumerable<OrganizationRelevantSystemDTO> RelevantSystems{ get; set; }
    }
}