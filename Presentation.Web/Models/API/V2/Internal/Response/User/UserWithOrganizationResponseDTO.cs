using System;

namespace Presentation.Web.Models.API.V2.Internal.Response.User
{
    public class UserWithOrganizationResponseDTO : UserWithApiAccessResponseDTO
    {
        public string OrgName { get; set; }
        protected UserWithOrganizationResponseDTO()
        {
        }

        public UserWithOrganizationResponseDTO(Guid uuid, string name, string email, bool apiAccess, string orgName)
            : base(uuid, name, email, apiAccess)
        {
            OrgName = orgName;
        }
    }
}