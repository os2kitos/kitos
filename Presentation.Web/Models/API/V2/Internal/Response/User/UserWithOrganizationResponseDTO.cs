using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Internal.Response.User
{
    public class UserWithOrganizationResponseDTO : UserWithApiAccessResponseDTO
    {
        public required string OrgName { get; set; }
        protected UserWithOrganizationResponseDTO()
        {
        }

        [SetsRequiredMembers]
        public UserWithOrganizationResponseDTO(Guid uuid, string name, string email, bool apiAccess, string orgName)
            : base(uuid, name, email, apiAccess)
        {
            OrgName = orgName;
        }
    }
}