using Core.DomainModel;

namespace Core.ApplicationServices
{
    public interface IAuthenticationService
    {
        bool HasReadAccess(int userId, IEntity entity);
        /// <summary>
        /// Checks if the user have write access to a given instance.
        /// </summary>
        /// <param name="user">The user id.</param>
        /// <param name="entity">The instance the user want write access to.</param>
        /// <returns>Returns true if the user have write access to the given instance, else false.</returns>
        bool HasWriteAccess(int userId, IEntity entity);
        bool IsGlobalAdmin(int userId);
        bool IsLocalAdmin(int userId);
        bool HasReadAccessOutsideContext(int userId);
        int GetCurrentOrganizationId(int userId);
        bool CanExecute(int userId, Feature feature);
    }
}
