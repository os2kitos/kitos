using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Internal.Response.User
{
    public class UserWithCrossOrganizationalRightsResponseDTO : UserWithApiAccessResponseDTO
    {
        public bool StakeholderAccess { get; set; }
        public required IEnumerable<string> OrganizationsWhereActive { get; set; }

        public UserWithCrossOrganizationalRightsResponseDTO()
        {
              
        }

        [SetsRequiredMembers]
        public UserWithCrossOrganizationalRightsResponseDTO(Guid uuid, string name, string email, bool apiAccess, bool stakeholderAccess, IEnumerable<string> organizationsWhereActive) : base(uuid, name, email, apiAccess)
        {
            StakeholderAccess = stakeholderAccess;
            OrganizationsWhereActive = organizationsWhereActive;
        }
    }
}