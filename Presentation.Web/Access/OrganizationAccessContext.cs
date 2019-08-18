using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Presentation.Web.Access
{
    public class OrganizationAccessContext
    {
        private readonly IUserContextFactory _userContextFactory;
        private readonly IGenericRepository<ItSystemRole> _systemRoleRepository;
        private readonly int _organizationId; //TODO: Consider if we should take it as input param in stead. This may lead to wrong answers

        public OrganizationAccessContext(
            IUserContextFactory userContextFactory,
            int organizationId)
        {
            _userContextFactory = userContextFactory;
            _organizationId = organizationId;
        }

        public bool AllowReads(int userId)
        {
            var result = false;

            var userContext = _userContextFactory.Create(userId, _organizationId);

            if (IsGlobalAdmin(userContext))
            {
                result = true;
            }
            else if (TargetOrganizationMatchesActiveOrganization(userContext))
            {
                result = true;
            }

            if (IsUserInMunicipality(userContext))
            {
                result = true;
            }

            return result;
        }

        public bool AllowReads(int userId, IEntity entity)
        {
            var result = false;
            //TODO: The "Active" context is always the left hand of the equation so maybe we should not model it this way or at least let the factory extract it from auth context (active user auth)
            var userContext = _userContextFactory.Create(userId, _organizationId);

            var user = userContext.User;
            if (IsGlobalAdmin(userContext))
            {
                result = true;
            }
            else if (IsContextBound(entity) == false || ActiveContextIsEntityContext(entity, userContext))
            {
                if (HasOwnership(entity, user))
                {
                    result = true;
                }
                else if (IsContextBound(entity) && ActiveContextIsEntityContext(entity, userContext))
                {
                    result = true;
                }
                else if (IsUserInMunicipality(userContext) && HasAssignedPublicAccess(entity))
                {
                    result = true;
                }
                else if (IsUserEntity(entity) && entity.Id == userId)
                {
                    result = true;
                }
            }

            return result;
        }

        public bool AllowUpdates(int userId, IEntity entity)
        {
            var result = false;

            //TODO: The "Active" context is always the left hand of the equation so maybe we should not model it this way or at least let the factory extract it from auth context (active user auth)
            var userContext = _userContextFactory.Create(userId, _organizationId);
            var user = userContext.User;
            if (IsGlobalAdmin(userContext))
            {
                result = true;
            }
            else if (HasAssignedWriteAccess(entity, user))
            {
                result = true;
            }
            else if (IsContextBound(entity) == false || ActiveContextIsEntityContext(entity, userContext))
            {
                if (IsLocalAdmin(userContext))
                {
                    result = true;
                }
                else if (HasModuleLevelWriteAccess(userContext, entity))
                {
                    result = true;
                }
                else if (IsUserEntity(entity) == false && HasOwnership(entity, user))
                {
                    result = true;
                }
                else if (IsUserEntity(entity) && (entity.Id == user.Id))
                {
                    result = true;
                }
            }

            return result && IsReadOnly(userContext) == false;
        }

        private static bool HasModuleLevelWriteAccess(IOrganizationalUserContext user, IEntity entity)
        {
            return user.HasModuleLevelAccessTo(entity);
        }

        private static bool IsUserEntity(IEntity entity)
        {
            return entity is User;
        }

        private static bool HasAssignedPublicAccess(IEntity entity)
        {
            return (entity as IHasAccessModifier)?.AccessModifier == AccessModifier.Public;
        }

        private static bool IsUserInMunicipality(IOrganizationalUserContext user)
        {
            return user.IsActiveInOrganizationOfType(OrganizationCategory.Municipality);
        }

        private bool TargetOrganizationMatchesActiveOrganization(IOrganizationalUserContext user)
        {
            return user.IsActiveInOrganization(_organizationId);
        }

        private static bool HasAssignedWriteAccess(IEntity entity, User user)
        {
            return entity.HasUserWriteAccess(user);
        }

        private static bool IsContextBound(IEntity entity)
        {
            return entity is IContextAware;
        }

        private static bool ActiveContextIsEntityContext(IEntity entity, IOrganizationalUserContext user)
        {
            return user.IsActiveInSameOrganizationAs((IContextAware)entity);
        }

        private static bool HasOwnership(IEntity ownedEntity, IEntity ownerEntity)
        {
            return ownedEntity.ObjectOwnerId == ownerEntity.Id;
        }

        private static bool IsGlobalAdmin(IOrganizationalUserContext user)
        {
            return user.HasRole(OrganizationRole.GlobalAdmin);
        }

        private static bool IsReadOnly(IOrganizationalUserContext user)
        {
            return user.HasRole(OrganizationRole.ReadOnly);
        }

        private static bool IsLocalAdmin(IOrganizationalUserContext user)
        {
            return user.HasRole(OrganizationRole.LocalAdmin);
        }
    }
}