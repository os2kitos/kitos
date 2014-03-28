using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IOrganizationService
    {
        Organization CreateOrganization(string name);
        Organization CreateMunicipality(string name);

        bool IsUserMember(User user, Organization organization);
    }
}