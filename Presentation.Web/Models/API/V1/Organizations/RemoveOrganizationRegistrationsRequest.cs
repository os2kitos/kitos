using Presentation.Web.Models.API.V1.Users;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class RemoveOrganizationRegistrationsRequest
    {
        [Required]
        public IEnumerable<AssignedRightDTO> Roles { get; set; }
        public IEnumerable<ObjectIdAndParentIdDTO> InternalPayments{ get; set; }
        public IEnumerable<ObjectIdAndParentIdDTO> ExternalPayments{ get; set; }
        public IEnumerable<ObjectIdAndParentIdDTO> ContractRegistrations{ get; set; }
        public IEnumerable<ObjectIdAndParentIdDTO> ResponsibleSystems{ get; set; }
        public IEnumerable<ObjectIdAndParentIdDTO> RelevantSystems{ get; set; }
    }
}