using System.Collections.Generic;
using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IOrganizationService
    {
        ICollection<Organization> GetByUser(User user);
        Organization CreateOrganization(string name, OrganizationType organizationType); // TODO Remove this method
        void AddDefaultOrgUnit(Organization org);
        bool IsUserMember(User user, Organization organization);
    }
}