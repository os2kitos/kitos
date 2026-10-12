using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Users.Write
{
    public class CreateUserParameters
    {
        public required User User { get; set; }
        public bool SendMailOnCreation { get; set; }
        public required IEnumerable<OrganizationRole> Roles { get; set; }
    }
}
