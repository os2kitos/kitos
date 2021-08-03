﻿using System.Collections.Generic;

namespace Presentation.Web.Models.Users
{
    public class UserWithCrossOrganizationalRightsDTO : UserWithApiAccessDTO
    {
        public bool StakeholderAccess { get; set; }
        public IEnumerable<string> OrganizationsWhereActive { get; set; }

        public UserWithCrossOrganizationalRightsDTO(int id, string fullName, string email, bool apiAccess, bool stakeholderAccess, IEnumerable<string> organizationsWhereActive) : base(id, fullName, email, apiAccess)
        {
            StakeholderAccess = stakeholderAccess;
            OrganizationsWhereActive = organizationsWhereActive;
        }
    }
}