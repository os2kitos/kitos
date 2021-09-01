using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Types;
using Presentation.Web.Models.API.V2.Types.Organization;

namespace Presentation.Web.Models.API.V2.Response.Organization
{
    public class OrganizationUserResponseDTO : IdentityNamePairResponseDTO
    {
        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// User's last name(s)
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// User's email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// User's phone number
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Determines if the user credentials can be used to request a KITOS API token
        /// </summary>
        public bool ApiAccess { get; set; }
        /// <summary>
        /// Roles assigned to the user within the context of the organization
        /// </summary>
        public IEnumerable<OrganizationUserRole> Roles { get; set; }
    }
}