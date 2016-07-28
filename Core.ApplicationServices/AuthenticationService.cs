using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Linq;
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
            var user = _userRepository.AsQueryable()
                .Single(x => x.Id == userId);
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
                .Single(x => x.Id == userId &&
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
                .Single(x => x.Id == userId &&
                    x.OrganizationRights.Any(
                        right => right.Role == OrganizationRole.LocalAdmin &&
                        right.OrganizationId == x.DefaultOrganizationId));

            return user != null;
        }

        public bool HasReadAccessOutsideContext(int userId)
        {
            var user = _userRepository.AsQueryable().Single(x => x.Id == userId);
            if (user.IsGlobalAdmin)
            {
                return true;
            }

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

                // check if user is part of target organization (he's trying to access)
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
                    return false;
                }

                // check if local admin in target organization
                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.LocalAdmin))
                {
                    return true;
                }

                // check if module admin in target organization and target entity is of the correct type
                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.ContractModuleAdmin)
                    && entity is IContractModule)
                {
                    return true;
                }

                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.OrganizationModuleAdmin)
                    && entity is IOrganizationModule)
                {
                    return true;
                }

                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.ProjectModuleAdmin)
                    && entity is IProjectModule)
                {
                    return true;
                }

                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.SystemModuleAdmin)
                    && entity is ISystemModule)
                {
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
                    return true;
                }
            }

            return false;
        }
    }
}
