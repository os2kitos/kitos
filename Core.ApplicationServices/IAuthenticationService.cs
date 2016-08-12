using Core.DomainModel;

namespace Core.ApplicationServices
{
    public interface IAuthenticationService
    {
        bool HasReadAccess(int userId, Entity entity);
        bool HasWriteAccess(int userId, Entity entity);
        bool IsGlobalAdmin(int userId);
        bool IsLocalAdmin(int userId, int organizationId);
        bool IsLocalAdmin(int userId);
        bool HasReadAccessOutsideContext(int userId);
        bool HasWriteAccess(int userId, object entity);
    }
}
