using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V2.Internal.Request.Organizations
{
    public class OrganizationMasterDataRolesRequestDTO
    {
        public ContactPersonRequestDTO ContactPerson { get; set; }
        public DataResponsibleRequestDTO DataResponsible { get; set; }
        public DataProtectionAdvisorRequestDTO DataProtectionAdvisor { get; set; }
    }
}