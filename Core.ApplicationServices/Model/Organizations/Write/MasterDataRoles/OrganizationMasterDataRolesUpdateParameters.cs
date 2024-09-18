using System;
using Core.Abstractions.Types;

namespace Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles
{
    public class OrganizationMasterDataRolesUpdateParameters
    {
        public Guid OrganizationUuid { get; set; }
        public Maybe<ContactPersonUpdateParameters> ContactPerson { get; set; }

        public Maybe<DataResponsibleUpdateParameters> DataResponsible { get; set; }
    }
}
