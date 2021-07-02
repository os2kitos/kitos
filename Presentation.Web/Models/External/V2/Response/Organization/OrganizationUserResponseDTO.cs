using System;
using System.Collections.Generic;
using System.Linq;
using Presentation.Web.Models.External.V2.Types;

namespace Presentation.Web.Models.External.V2.Response.Organization
{
    public class OrganizationUserResponseDTO : IdentityNamePairResponseDTO
    {
        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; }
        /// <summary>
        /// User's last name(s)
        /// </summary>
        public string LastName { get; }
        /// <summary>
        /// User's email
        /// </summary>
        public string Email { get; }
        /// <summary>
        /// User's phone number
        /// </summary>
        public string PhoneNumber { get; }
        /// <summary>
        /// Determines if the user credentials can be used to request a KITOS API token
        /// </summary>
        public bool APIAccess { get; }
        /// <summary>
        /// Roles assigned to the user within the context of the organization
        /// </summary>
        public IEnumerable<OrganizationUserRole> Roles { get; }

        public OrganizationUserResponseDTO(Guid uuid, string name, string firstName, string lastName, string email, bool apiAccess, string phoneNumber, IEnumerable<OrganizationUserRole> roles)
            : base(uuid, name)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            APIAccess = apiAccess;
            PhoneNumber = phoneNumber;
            Roles = roles.ToList().AsReadOnly();
        }
    }
}