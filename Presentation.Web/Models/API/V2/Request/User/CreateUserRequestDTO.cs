using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.User
{
    public class CreateUserRequestDTO : BaseUserRequestDTO
    {
        public bool SendMailOnCreation { get; set; }
        public IEnumerable<OrganizationRoleChoice> Roles { get; set; }
    }
}