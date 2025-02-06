using System;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationMasterDataRoles
    {
        public Guid OrganizationUuid { get; set; }
        public ContactPerson ContactPerson { get; set; }
        public DataResponsible DataResponsible { get; set; }
        public DataProtectionAdvisor DataProtectionAdvisor { get; set; }
    }
}
