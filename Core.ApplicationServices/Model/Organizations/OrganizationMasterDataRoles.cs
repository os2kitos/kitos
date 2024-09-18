using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationMasterDataRoles
    {
        public Guid OrganizationUuid { get; set; }
        public ContactPerson ContactPerson { get; set; }
    }
}
