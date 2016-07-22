using Core.DomainModel;

namespace Core.ApplicationServices
{
    public interface IAuthenticationService
    {
        bool HasReadAccess(User user, Organization loggedIntoOrganization, Entity entity);
        bool HasWriteAccess(User user, Organization loggedIntoOrganization, Entity entity);
        bool IsGlobalAdmin(User user);
        bool IsLocalAdmin(User user, Organization organization);
    }
}
