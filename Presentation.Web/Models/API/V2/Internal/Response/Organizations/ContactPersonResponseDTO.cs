using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class ContactPersonResponseDTO: OrganizationMasterDataRoleResponseDTO
    {
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }
        }
    }