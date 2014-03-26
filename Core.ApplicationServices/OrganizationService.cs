using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class OrganizationService : IOrganizationService
    {
        public Organization CreateOrganization(string name)
        {
            var org = new Organization
            {
                Name = name,
                Config = Config.Default
            };

            org.OrgUnits.Add(new OrganizationUnit()
            {
                Name = org.Name,
            });

            return org;
        }

        public Organization CreateMunicipality(string name)
        {
            var org = CreateOrganization(name);
            org.Type = OrganizationType.Municipality;

            return org;
        }

        public bool IsUserMember(User user, Organization organization)
        {
            throw new System.NotImplementedException(); //TODO
        }
    }
}