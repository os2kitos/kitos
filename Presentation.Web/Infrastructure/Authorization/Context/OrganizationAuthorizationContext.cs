using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;

namespace Presentation.Web.Infrastructure.Authorization.Context
{
    public class OrganizationAuthorizationContext : IAuthorizationContext
    {
        private readonly IOrganizationalUserContext _activeUserContext;

        public OrganizationAuthorizationContext(IOrganizationalUserContext activeUserContext)
        {
            _activeUserContext = activeUserContext;
        }

        public bool AllowGlobalReadAccess()
        {
            return _activeUserContext.HasRole(OrganizationRole.GlobalAdmin);
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
                else if (IsUserInMunicipality() && EntityAllowsCrossOrganizationRead(entity))
                {
                    result = true;
                }
            }

            return result;
        }

        public bool AllowCreate<T>()
        {
            if (IsReadOnly())
            {
                return false;
            }

            if (MatchType<T, ItSystem>())
            {
                return IsGlobalAdmin();
            }

            //NOTE: Once we migrate more types, this will be extended
            return true;
        }

        public bool AllowCreate<T>(IEntity entity)
        {
            return
                AllowCreate<T>() &&
                AllowModify(entity); //NOTE: Ensures backwards compatibility as long as some terms are yet to be fully migrated
        }

        public bool AllowModify(IEntity entity)
        {
            var result = false;

            var ignoreReadOnlyRole = false;

            if (IsGlobalAdmin())
            {
                result = true;
            }
            else if (EntityEqualsActiveUser(entity))
            {
                ignoreReadOnlyRole = true;
                result = true;
            }
            else if (IsContextBound(entity))
            {
                if (ActiveContextIsEntityContext(entity))
                {
                    result =
                        IsLocalAdmin() ||
                        AllowWritesToEntity(entity) ||
                        HasAssignedWriteAccess(entity);
                }
            }
            else
            {
                result = AllowWritesToEntity(entity);
            }

            //If result is TRUE, this can be negated if read-only is not ignored AND user is marked as read-only
            return result && (ignoreReadOnlyRole || IsReadOnly() == false);
        }

        public bool AllowDelete(IEntity entity)
        {
            var result = false;
            if (AllowModify(entity))
            {
                switch (entity)
                {
                    case ItSystem _:
                        result =
                            IsGlobalAdmin() ||
                            (IsLocalAdmin() && ActiveContextIsEntityContext(entity));
                        break;
                    default:
                        result = true;
                        break;
                }
            }

            return result;
        }

        public bool AllowEntityVisibilityControl(IEntity entity)
        {
            return AllowModify(entity) && _activeUserContext.CanChangeVisibilityOf(entity);
        }

        private bool AllowWritesToEntity(IEntity entity)
        {
            var result = false;

            if (HasModuleLevelWriteAccess(entity))
            {
                result = true;
            }
            else if (IsUserEntity(entity) == false && HasOwnership(entity))
            {
                result = true;
            }

            return result;
        }

        private bool HasModuleLevelWriteAccess(IEntity entity)
        {
            return _activeUserContext.HasModuleLevelAccessTo(entity);
        }

        private static bool IsUserEntity(IEntity entity)
        {
            return entity is User;
        }

        private static bool EntityAllowsCrossOrganizationRead(IEntity entity)
        {
            //Only return true if entity supports cross-organization sharing and access is marked as public
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

        private bool HasAssignedWriteAccess(IEntity entity)
        {
            return _activeUserContext.HasAssignedWriteAccess(entity);
        }

        private static bool IsContextBound(IEntity entity)
        {
            return entity is IContextAware || entity is IHasOrganization;
        }

        private bool ActiveContextIsEntityContext(IEntity entity)
        {
            return _activeUserContext.IsActiveInSameOrganizationAs(entity);
        }

        private bool HasOwnership(IEntity ownedEntity)
        {
            return _activeUserContext.HasOwnership(ownedEntity);
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
            return IsUserEntity(entity) && entity.Id == _activeUserContext.UserId;
        }

        private static bool MatchType<TLeft, TRight>()
        {
            return typeof(TLeft) == typeof(TRight);
        }
    }
}