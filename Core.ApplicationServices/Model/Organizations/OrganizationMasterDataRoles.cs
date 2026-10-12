using System;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationMasterDataRoles
    {
        public Guid OrganizationUuid { get; set; }
        public required ContactPerson ContactPerson { get; set; }
        public required DataResponsible DataResponsible { get; set; }
        public required DataProtectionAdvisor DataProtectionAdvisor { get; set; }
    }
}
