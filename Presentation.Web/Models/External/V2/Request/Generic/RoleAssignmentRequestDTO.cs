using System;

namespace Presentation.Web.Models.External.V2.Request.Generic
{
    public class RoleAssignmentRequestDTO
    {
        public Guid RoleUuid { get; set; }
        public Guid UserUuid { get; set; }
    }
}