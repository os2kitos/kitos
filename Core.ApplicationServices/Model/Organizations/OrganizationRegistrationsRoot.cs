using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationsRoot
    {
        public List<OrganizationRegistrationDetails> Roles { get; set; }
        public List<OrganizationRegistrationDetails> Payments { get; set; }
        public List<OrganizationRegistrationDetails> InternalContractRegistrations { get; set; }
        public List<OrganizationRegistrationDetails> ExternalContractRegistrations { get; set; }
        public List<OrganizationRegistrationDetails> RelevantSystemRegistrations { get; set; }
    }
}
