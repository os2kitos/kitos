using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationMasterDataRolesUpdateParameters
    {
        public OptionalValueChange<ContactPerson> ContactPerson { get; set; }
    }
}
