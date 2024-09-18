using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationMasterDataRolesUpdateParameters
    {
        public Guid OrganizationUuid { get; set; }
        public Maybe<ContactPersonUpdateParameters> ContactPerson { get; set; }
    }
}
