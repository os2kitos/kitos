using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IAdminService
    {
        void MakeLocalAdmin(User user, Organization organization);

        bool IsGlobalAdmin(User user);
        bool IsLocalAdmin(User user, Organization organization);
    }
}