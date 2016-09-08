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

        public bool HasReadAccessOutsideContext(User user)
        {
            if(user == null)
                throw new AuthenticationException("User is null");

            if (user.IsGlobalAdmin)
                return true;

            // if the user is logged into an organization that allows sharing,
            // then the user have read access outside the context.
            return user.DefaultOrganization.Type.Category == OrganizationCategory.Municipality;
        }

        public bool HasReadAccessOutsideContext(int userId)
        {
            var user = _userRepository.AsQueryable().SingleOrDefault(x => x.Id == userId);
            return HasReadAccessOutsideContext(user);
        }

        public bool HasReadAccess(int userId, Entity entity)
        {
            var user = _userRepository.AsQueryable().FirstOrDefault(x => x.Id == userId);
            return HasReadAccess(user, entity);
        }


        /// <summary>
        /// Checks if the user have read access to a given instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="entity">The instance the user want read access to.</param>
        /// <returns>Returns true if the user have read access to the given instance, else false.</returns>
        public bool HasReadAccess(User user, Entity entity)
        {
            if (user == null)
                return false;

            // global admin always have access
            if (user.IsGlobalAdmin)
                return true;

            var awareEntity = entity as IContextAware;
            if (awareEntity == null)
                return false;

            var loggedIntoOrganizationId = user.DefaultOrganizationId.GetValueOrDefault();

            // check if user is part of target organization (he's trying to access)
            if (awareEntity.IsInContext(loggedIntoOrganizationId))
                // users part of an orgaization have read access to all entities inside the organization
                return true;
            if (user.DefaultOrganization.Type.Category == OrganizationCategory.Municipality)
                // organizations of type OrganizationCategory.Municipality have read access to other organizations
                return true;

            return false;
        }

        /// <summary>
        /// Checks if the user have write access to a given instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="entity">The instance the user want read access to.</param>
        /// <returns>Returns true if the user have write access to the given instance, else false.</returns>
        public bool HasWriteAccess(User user, Entity entity)
        {
            var loggedIntoOrganizationId = user.DefaultOrganizationId.GetValueOrDefault();

            // check if global admin
            // global admin always have access
            if (user.IsGlobalAdmin)
                return true;

            // check if user is in context
            var awareEntity = entity as IContextAware;
            if (awareEntity == null)
                return false;

            // check if user is part of target organization (he's trying to access)
            if (!awareEntity.IsInContext(loggedIntoOrganizationId))
                return false;

            // check if local admin in target organization
            if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.LocalAdmin))
                return true;

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

            if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.ProjectModuleAdmin) && entity is IProjectModule)
                return true;

            if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.SystemModuleAdmin) && entity is ISystemModule)
                return true;

            if (user.DefaultOrganization.Rights.Any(x => x.Role == OrganizationRole.ReportModuleAdmin) && entity is IReportModule)
                return true;

            // check if user has a write role on the target entity
            // TODO HasUserWriteAccess isn't ideal, it should be rewritten into checking roles as the other checks are done here
            if (entity.HasUserWriteAccess(user))
                return true;

            // check if user is object owner
            if (entity.ObjectOwnerId == user.Id)
                return true;

            return false;
        }
    }
}
