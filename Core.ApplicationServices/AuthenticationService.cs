using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using System.Linq;

namespace Core.ApplicationServices
{
    public class AuthenticationService : IAuthenticationService
    {
        public bool IsGlobalAdmin(User user)
        {
            return user.IsGlobalAdmin;
        }

        public bool IsLocalAdmin(User user, Organization organization)
        {
            return user.OrganizationRights.Any(right => right.Role == OrganizationRole.LocalAdmin && right.OrganizationId == organization.Id);
        }

        /// <summary>
        /// Checks if the user have read access to a given instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="loggedIntoOrganization">The organization the user is logged into.</param>
        /// <param name="entity">The instance the user want read access to.</param>
        /// <returns>Returns true if the user have read access to the given instance, else false.</returns>
        public bool HasReadAccess(User user, Organization loggedIntoOrganization, Entity entity)
        {
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
                if (awareEntity.IsInContext(loggedIntoOrganization.Id))
                {
                    // users part of an orgaization have read access to all entities inside the organization
                    return true;
                }
                else // if not, check if organization, he's logged into, allows sharing
                {
                    if (loggedIntoOrganization.Type.Category == OrganizationCategory.Municipality)
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
        /// <param name="user">The user.</param>
        /// <param name="loggedIntoOrganization">The organization the user is logged into.</param>
        /// <param name="entity">The instance the user want read access to.</param>
        /// <returns>Returns true if the user have write access to the given instance, else false.</returns>
        public bool HasWriteAccess(User user, Organization loggedIntoOrganization, Entity entity)
        {
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
                if (!awareEntity.IsInContext(loggedIntoOrganization.Id))
                {
                    return false;
                }

                // check if local admin in target organization
                if (loggedIntoOrganization.Rights.Any(x => x.Role == OrganizationRole.LocalAdmin))
                {
                    return true;
                }

                // check if module admin in target organization and target entity is of the correct type
                if (loggedIntoOrganization.Rights.Any(x => x.Role == OrganizationRole.ContractModuleAdmin)
                    && entity is IContractModule)
                {
                    return true;
                }

                if (loggedIntoOrganization.Rights.Any(x => x.Role == OrganizationRole.OrganizationModuleAdmin)
                    && entity is IOrganizationModule)
                {
                    return true;
                }

                if (loggedIntoOrganization.Rights.Any(x => x.Role == OrganizationRole.ProjectModuleAdmin)
                    && entity is IProjectModule)
                {
                    return true;
                }

                if (loggedIntoOrganization.Rights.Any(x => x.Role == OrganizationRole.SystemModuleAdmin)
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
