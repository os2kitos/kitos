﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V2.Internal.Request.Organizations
{
    public class ContactPersonRequestDTO: OrganizationMasterDataRoleRequestDTO
    {
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
    }
}