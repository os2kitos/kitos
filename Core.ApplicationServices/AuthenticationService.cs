using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Linq;
using System.Security.Authentication;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;

namespace Core.ApplicationServices
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IGenericRepository<User> _userRepository;

        public readonly IFeatureChecker _featureChecker;

        public AuthenticationService(IGenericRepository<User> userRepository, IFeatureChecker featureChecker)
        {
            _userRepository = userRepository;
            _featureChecker = featureChecker;
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
                if (user.DefaultOrganization.Type.Category == OrganizationCategory.Municipality)
                {
                    // organizations of type OrganizationCategory.Municipality have read access to other organizations
                    return true;
                }
            }

            // User is a special case
            if (entity is User)
                return CheckUserSpecialCase((User) entity, user);

            return false;
        }

        private static bool CheckUserSpecialCase(User entity, User user)
        {
            if (user.IsLocalAdmin)
                return true;

            // check if the user is trying edit himself
            // a user always has write access to himself
            return entity.Id == user.Id;
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
            AssertUserIsNotNull(user);
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
                var awareEntity = (IContextAware) entity;

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
                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.ContractModuleAdmin) && entity is IContractModule)
                    return true;

                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.OrganizationModuleAdmin) && entity is IOrganizationModule)
                    return true;

                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.ProjectModuleAdmin) && entity is IProjectModule)
                    return true;

                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.SystemModuleAdmin) && entity is ISystemModule)
                    return true;

                if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.ReportModuleAdmin) && entity is IReportModule)
                    return true;

                // check if user has a write role on the target entity
                if (entity.HasUserWriteAccess(user))
                    return true;

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
                if (entity.HasUserWriteAccess(user))
                {
                    return true;
                }
            }

            // User is a special case
            if (entity is User)
                return CheckUserSpecialCase((User) entity, user);

            // all white-list checks failed, deny access
            return false;
        }

        public int GetCurrentOrganizationId(int userId)
        {
            var user = _userRepository.AsQueryable().Single(x => x.Id == userId);
            var loggedIntoOrganizationId = user.DefaultOrganizationId.Value;
            return loggedIntoOrganizationId;
        }

        public bool CanExecute(int userId, Feature feature)
        {
            var user = _userRepository.AsQueryable().Single(x => x.Id == userId);
            return _featureChecker.CanExecute(user, feature);
        }

        // ReSharper disable once UnusedParameter.Local
        private void AssertUserIsNotNull(User user)
        {
            if (user == null)
                throw new AuthenticationException("User is null");
        }
    }
}
