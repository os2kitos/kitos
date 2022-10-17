using Core.ApplicationServices.Model.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationsChangeParameters
    {
        public UserRightsChangeParameters Roles { get; set; }
    }
}
