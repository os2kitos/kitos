using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.DomainServices
{
    public interface IAdminService
    {
        OrganizationRight MakeLocalAdmin(User user, Organization organization, User kitosUser);
        void RemoveLocalAdmin(User user, Organization organization);
    }
}
