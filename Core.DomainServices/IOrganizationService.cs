using System.Collections.Generic;
using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IOrganizationService
    {
        ICollection<Organization> GetByUser(User user);
        void AddDefaultOrgUnit(Organization org);
        bool IsUserMember(User user, Organization organization);
    }
}