using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationDetailsWithObjectData : OrganizationRegistrationDetails
    {
        public int ObjectId { get; set; }
        public string ObjectName { get; set; }
    }
}
