using System.Collections.Generic;
using Core.DomainModel.Organization;

namespace Presentation.Web.Models.API.V1.Users
{
    public class OrganizationUserRoleAssignmentsDTO
    {
        public IEnumerable<OrganizationRole> AdministrativeAccessRoles { get; set; }
        public IEnumerable<AssignedRightDTO> Rights { get; set; }
    }
}