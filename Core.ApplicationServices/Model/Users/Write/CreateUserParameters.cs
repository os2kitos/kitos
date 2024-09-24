using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Users.Write
{
    public class CreateUserParameters
    {
        public User User { get; set; }
        public bool SendMailOnCreation { get; set; }
        public IEnumerable<OrganizationRole> Roles { get; set; }
    }
}
