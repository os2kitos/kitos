using System;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;

namespace Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit
{
    public class OrganizationUnitRolesResponseDTO
    {
        public required ExtendedRoleAssignmentResponseDTO RoleAssignment { get; set; }

        public required Guid OrganizationUnitUuid { get; set; }

        public required string OrganizationUnitName { get; set; }
    }
}