using System.Collections.Generic;
using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IOrganizationService
    {
        ICollection<Organization> GetByUser(User user);
        void SetupDefaultOrganization(Organization org, User objectOwner);
    }
}