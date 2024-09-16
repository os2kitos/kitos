using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationUpdateParameters
    {
        public OptionalValueChange<string> Cvr { get; set; }
        public OptionalValueChange<string> Phone { get; set; }
        public OptionalValueChange<string> Address { get; set; }
        public OptionalValueChange<string> Email { get; set; }
    }
}
