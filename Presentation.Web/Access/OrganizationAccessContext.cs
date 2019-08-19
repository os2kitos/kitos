using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Presentation.Web.Access
{
    public class OrganizationAccessContext : IAccessContext
    {
        private readonly IOrganizationalUserContext _activeUserContext;

        public OrganizationAccessContext(IOrganizationalUserContext activeUserContext)
        {
            _activeUserContext = activeUserContext;
        }

        public bool AllowReadsWithinOrganization(int organizationId)
        {
            var result = false;

            if (IsGlobalAdmin())
            {
                result = true;
            }
            else if (TargetOrganizationMatchesActiveOrganization(organizationId))
            {
                result = true;
            }
            else if (IsUserInMunicipality())
            {
                //TODO: Ask question: Verify this. Seems a bit broad. Is there no requirement that the other org is a municipality?
                result = true;
            }

            return result;
        }

        public bool AllowReads(IEntity entity)
        {
            var result = false;

            if (IsGlobalAdmin())
            {
                result = true;
            }
            else if (EntityEqualsActiveUser(entity))
            {
                result = true;
            }
            else if (IsContextBound(entity))
            {
                if (ActiveContextIsEntityContext(entity))
                {
                    result = true;
                }
                else if (IsUserInMunicipality() && HasAssignedPublicAccess(entity))
                {
                    result = true;
                }
            }

            return result;
        }

        public bool AllowUpdates(IEntity entity)
        {
            var result = false;

            var user = _activeUserContext.User;
            var ignoreReadOnlyRole = false;

            if (IsGlobalAdmin())
            {
                result = true;
            }
            else if (HasAssignedWriteAccess(entity, user)) //TODO: Ask question: Should it not be "in context" for "forretningsroller"?
            {
                result = true;
            }
            else if (IsContextBound(entity))
            {
                if (ActiveContextIsEntityContext(entity))
                {
                    var allow = AllowWritesToEntity(entity);
                    result = allow.result;
                    ignoreReadOnlyRole = allow.ignoreReadOnlyRole;
                }
            }
            else
            {
                var allow = AllowWritesToEntity(entity);
                result = allow.result;
                ignoreReadOnlyRole = allow.ignoreReadOnlyRole;
            }

            //If result is TRUE, this can be negated if read-only is not ignored AND user is marked as read-only
            return result && (ignoreReadOnlyRole || IsReadOnly() == false);
        }

        private (bool result, bool ignoreReadOnlyRole) AllowWritesToEntity(IEntity entity)
        {
            var ignoreReadOnlyRole = false;
            var result = false;
            var user = _activeUserContext.User;

            if (IsLocalAdmin())
            {
                result = true;
            }
            else if (HasModuleLevelWriteAccess(entity))
            {
                result = true;
            }
            else if (IsUserEntity(entity) == false && HasOwnership(entity, user))
            {
                result = true;
            }
            else if (EntityEqualsActiveUser(entity))
            {
                ignoreReadOnlyRole = true; //Ignore read-only since user is editing own entity properties
                result = true;
            }

            return (result, ignoreReadOnlyRole);
        }

        private bool HasModuleLevelWriteAccess(IEntity entity)
        {
            return _activeUserContext.HasModuleLevelAccessTo(entity);
        }

        private static bool IsUserEntity(IEntity entity)
        {
            return entity is User;
        }

        private static bool HasAssignedPublicAccess(IEntity entity)
        {
            return (entity as IHasAccessModifier)?.AccessModifier == AccessModifier.Public;
        }

        private bool IsUserInMunicipality()
        {
            return _activeUserContext.IsActiveInOrganizationOfType(OrganizationCategory.Municipality);
        }

        private bool TargetOrganizationMatchesActiveOrganization(int targetOrganizationId)
        {
            return _activeUserContext.IsActiveInOrganization(targetOrganizationId);
        }

        private static bool HasAssignedWriteAccess(IEntity entity, User user)
        {
            return entity.HasUserWriteAccess(user);
        }

        private static bool IsContextBound(IEntity entity)
        {
            return entity is IContextAware;
        }

        private bool ActiveContextIsEntityContext(IEntity entity)
        {
            return _activeUserContext.IsActiveInSameOrganizationAs((IContextAware)entity);
        }

        private static bool HasOwnership(IEntity ownedEntity, IEntity ownerEntity)
        {
            return ownedEntity.ObjectOwnerId == ownerEntity.Id;
        }

        private bool IsGlobalAdmin()
        {
            return _activeUserContext.HasRole(OrganizationRole.GlobalAdmin);
        }

        private bool IsReadOnly()
        {
            return _activeUserContext.HasRole(OrganizationRole.ReadOnly);
        }

        private bool IsLocalAdmin()
        {
            return _activeUserContext.HasRole(OrganizationRole.LocalAdmin);
        }

        private bool EntityEqualsActiveUser(IEntity entity)
        {
            return IsUserEntity(entity) && entity.Id == _activeUserContext.User.Id;
        }
    }
}