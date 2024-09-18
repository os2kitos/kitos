using System;

namespace Presentation.Web.Models.API.V2.Request.OrganizationUnit
{
    public class BaseOrganizationUnitRoleAssignmentRequestDTO
    {
        public Guid RoleUuid { get; set; }
        public Guid UserUuid { get; set; }
    }
}