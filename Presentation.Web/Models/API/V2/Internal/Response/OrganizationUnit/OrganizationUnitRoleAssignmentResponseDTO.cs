using System;

namespace Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit
{
    public class OrganizationUnitRoleAssignmentResponseDTO
    {
        public Guid RoleUuid { get; set; }
        public Guid UserUuid { get; set; }
    }
}