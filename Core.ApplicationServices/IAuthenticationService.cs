using Core.DomainModel;

namespace Core.ApplicationServices
{
    public interface IAuthenticationService
    {
        bool HasReadAccess(int userId, Entity entity);
        bool HasReadAccess(User user, Entity entity);
        bool IsGlobalAdmin(int userId);
        bool IsLocalAdmin(int userId, int organizationId);
        bool IsLocalAdmin(int userId);
        bool HasReadAccessOutsideContext(int userId);
        bool HasReadAccessOutsideContext(User user);

        /// <summary>
        /// Checks if the user have write access to a given instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="entity">The instance the user want read access to.</param>
        /// <returns>Returns true if the user have write access to the given instance, else false.</returns>
        bool HasWriteAccess(User user, Entity entity);
    }
}
