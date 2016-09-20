using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Linq;
using System.Security.Authentication;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IGenericRepository<User> _userRepository;

        public AuthenticationService(IGenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public bool IsGlobalAdmin(int userId)
        {
            var user = _userRepository.GetByKey(userId);
            return user.IsGlobalAdmin;
        }

        /// <summary>
        /// Checks if the user is local admin in a respective organization.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public bool IsLocalAdmin(int userId, int organizationId)
        {
            var user = _userRepository.AsQueryable()
                .SingleOrDefault(x => x.Id == userId &&
                    x.OrganizationRights.Any(
                        right => right.Role == OrganizationRole.LocalAdmin &&
                        right.OrganizationId == organizationId));

            return user != null;
        }

        /// <summary>
        /// Checks if the user is local admin in the current organization.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsLocalAdmin(int userId)
        {
            var user = _userRepository.AsQueryable()
                .SingleOrDefault(x => x.Id == userId &&
                    x.OrganizationRights.Any(
                        right => right.Role == OrganizationRole.LocalAdmin &&
                        right.OrganizationId == x.DefaultOrganizationId));

            return user != null;
        }

        public bool HasReadAccessOutsideContext(int userId)
        {
            var user = _userRepository.GetByKey(userId);

            if (user.IsGlobalAdmin)
                return true;

            // if the user is logged into an organization that allows sharing,
            // then the user have read access outside the context.
            return user.DefaultOrganization.Type.Category == OrganizationCategory.Municipality;
        }

        /// <summary>
        /// Checks if the user have read access to a given instance.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="entity">The instance the user want read access to.</param>
        /// <returns>Returns true if the user have read access to the given instance, else false.</returns>
        public bool HasReadAccess(int userId, Entity entity)
        {
            var user = _userRepository.AsQueryable().Single(x => x.Id == userId);
            var loggedIntoOrganizationId = user.DefaultOrganizationId.Value;
            // check if global admin
            if (user.IsGlobalAdmin)
            {
                // global admin always have access
                return true;
            }

            if (entity is IContextAware) // TODO I don't like this impl
            {
                var awareEntity = entity as IContextAware;

                // check if user is part of the target organization (he's trying to access)
                if (awareEntity.IsInContext(loggedIntoOrganizationId))
                {
                    // users part of an orgaization have read access to all entities inside the organization
                    return true;
                }
                else // if not, check if organization, he's logged into, allows sharing
                {
                    if (user.DefaultOrganization.Type.Category == OrganizationCategory.Municipality)
                    {
                        // organizations of type OrganizationCategory.Municipality have read access to other organizations
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the user have write access to a given instance.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="entity">The instance the user want read access to.</param>
        /// <returns>Returns true if the user have write access to the given instance, else false.</returns>
        public bool HasWriteAccess(int userId, Entity entity)
        {
            var user = _userRepository.AsQueryable().Single(x => x.Id == userId);
            var loggedIntoOrganizationId = user.DefaultOrganizationId.Value;

            // check if global admin
            if (user.IsGlobalAdmin)
            {
                // global admin always have access
                return true;
            }

            // check if user is in context
            if (entity is IContextAware) // TODO I don't like this impl
            {
                var awareEntity = entity as IContextAware;

                // check if user is part of target organization (he's trying to access)
                if (!awareEntity.IsInContext(loggedIntoOrganizationId))
                {
                    // Users are not allowd to access objects outside their current context,
                    // even if they have access in the other context.
                    // Then they must switch context and try again.
                    return false;
                }

                // check if user is local admin in target organization
                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.LocalAdmin))
                {
                    // local admins have write access to everything within the context
                    return true;
                }

                // check if module admin in target organization and target entity is of the correct type
                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.ContractModuleAdmin)
                    && entity is IContractModule)
                {
                    // contract admins have write access to everything deemed part of the contract module
                    return true;
                }

                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.OrganizationModuleAdmin)
                    && entity is IOrganizationModule)
                {
                    // organization admins have write access to everything deemed part of the organization module
                    return true;
                }

                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.ProjectModuleAdmin)
                    && entity is IProjectModule)
                {
                    // project admins have write access to everything deemed part of the project module
                    return true;
                }

                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.SystemModuleAdmin)
                    && entity is ISystemModule)
                {
                    // system admins have write access to everything deemed part of the system module
                    return true;
                }

                // check if user has a write role on the target entity
                if (entity.HasUserWriteAccess(user)) // TODO HasUserWriteAccess isn't ideal, it should be rewritten into checking roles as the other checks are done here
                {
                    return true;
                }

                // check if user is object owner
                if (entity.ObjectOwnerId == user.Id)
                {
                    // object owners have write access to their objects if they're within the context,
                    // else they'll have to switch to the correct context and try again
                    return true;
                }
            }
            else // the entity is not aware of its context
            {
                // check if user is object owner
                if (entity.ObjectOwnerId == user.Id)
                {
                    // the entity is unaware of its context,
                    // so our only option is to allow the object owner write access
                    return true;
                }
            }

            // User is a special case
            if (entity is User)
            {
                var userEntity = entity as User;

                // check if the user is trying edit himself
                if (userEntity.Id == user.Id)
                {
                    // a user always has write access to himself
                    return true;
                }
            }

            // all white-list checks failed, deny access
            return false;
        }

        public int GetCurrentOrganizationId(int userId)
        {
            var user = _userRepository.AsQueryable().Single(x => x.Id == userId);
            var loggedIntoOrganizationId = user.DefaultOrganizationId.Value;
            return loggedIntoOrganizationId;
        }

        // ReSharper disable once UnusedParameter.Local
        private void AssertUserIsNotNull(User user)
        {
            if (user == null)
                throw new AuthenticationException("User is null");
        }
    }
}
