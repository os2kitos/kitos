using System;
using Core.Abstractions.Types;

namespace Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles
{
    public class OrganizationMasterDataRolesUpdateParameters
    {
        public Guid OrganizationUuid { get; set; }
        public required Maybe<ContactPersonUpdateParameters> ContactPerson { get; set; }
        public required Maybe<DataResponsibleUpdateParameters> DataResponsible { get; set; }
        public required Maybe<DataProtectionAdvisorUpdateParameters> DataProtectionAdvisor { get; set; }
    }
}
