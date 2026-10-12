using System;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;

namespace Presentation.Web.Models.API.V2.Response.Organization
{
    public class OrganizationMasterDataRolesResponseDTO
    {
        public Guid OrganizationUuid { get; set; }
        public required ContactPersonResponseDTO ContactPerson { get; set; }
        public required DataResponsibleResponseDTO DataResponsible { get; set; }
        public required DataProtectionAdvisorResponseDTO DataProtectionAdvisor { get; set; }
    }
}