using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;

namespace Presentation.Web.Models.API.V2.Response.Organization
{
    public class OrganizationMasterDataRolesResponseDTO
    {
        public ContactPersonResponseDTO ContactPerson { get; set; }
    }
}