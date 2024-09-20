using Presentation.Web.Models.API.V2.Request.User;
using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.User
{
    public class UserResponseDTO
    {
        public Guid Uuid { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DefaultUserStartPreferenceChoice DefaultUserStartPreference { get; set; }
        public bool? HasApiAccess { get; set; }
        public bool HasStakeHolderAccess { get; set; }
        public IEnumerable<OrganizationRoleChoice> Roles { get; set; }
    }
}