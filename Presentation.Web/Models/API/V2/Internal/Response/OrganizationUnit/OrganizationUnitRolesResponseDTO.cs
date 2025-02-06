using System;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;

namespace Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit
{
    public class OrganizationUnitRolesResponseDTO
    {
        public ExtendedRoleAssignmentResponseDTO RoleAssignment { get; set; }

        public Guid OrganizationUnitUuid { get; set; }

        public string OrganizationUnitName { get; set; }
    }
}