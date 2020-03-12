using System;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Authorization.Policies;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Authorization
{
    public class OrganizationAuthorizationContext : IAuthorizationContext, IPermissionVisitor
    {
        private readonly IOrganizationalUserContext _activeUserContext;
        private readonly IEntityTypeResolver _typeResolver;
        private readonly IModuleModificationPolicy _moduleLevelAccessPolicy;
        private readonly IGlobalReadAccessPolicy _globalReadAccessPolicy;
        private readonly IModuleCreationPolicy _typeCreationPolicy;

        public OrganizationAuthorizationContext(
            IOrganizationalUserContext activeUserContext,
            IEntityTypeResolver typeResolver,
            IModuleModificationPolicy moduleLevelAccessPolicy,
            IGlobalReadAccessPolicy globalReadAccessPolicy,
            IModuleCreationPolicy typeCreationPolicy)
        {
            _activeUserContext = activeUserContext;
            _typeResolver = typeResolver;
            _moduleLevelAccessPolicy = moduleLevelAccessPolicy;
            _globalReadAccessPolicy = globalReadAccessPolicy;
            _typeCreationPolicy = typeCreationPolicy;
        }

        public CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccess()
        {
            if (IsGlobalAdmin())
            {
                return CrossOrganizationDataReadAccessLevel.All;
            }

            return IsUserInMunicipality() ?
                CrossOrganizationDataReadAccessLevel.Public :
                CrossOrganizationDataReadAccessLevel.None;
        }

        public OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId)
        {
            if (TargetOrganizationMatchesActiveOrganization(organizationId))
            {
                return OrganizationDataReadAccessLevel.All;
            }

            switch (GetCrossOrganizationReadAccess())
            {
                case CrossOrganizationDataReadAccessLevel.Public:
                    return OrganizationDataReadAccessLevel.Public;
                case CrossOrganizationDataReadAccessLevel.All:
                    return OrganizationDataReadAccessLevel.All;
                default:
                    return OrganizationDataReadAccessLevel.None;
            }
        }

        public EntityReadAccessLevel GetReadAccessLevel<T>()
        {
            var entityType = _typeResolver.Resolve(typeof(T));
            return GetReadAccessLevel(entityType);
        }

        public bool AllowReads(IEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (EntityEqualsActiveUser(entity))
            {
                return true;
            }

            var entityType = _typeResolver.Resolve(entity.GetType());

            var readAccessLevel = GetReadAccessLevel(entityType);
            switch (readAccessLevel)
            {
                case EntityReadAccessLevel.None:
                    return false;
                case EntityReadAccessLevel.OrganizationOnly:
                    return ActiveContextIsEntityContext(entity);
                case EntityReadAccessLevel.OrganizationAndPublicFromOtherOrganizations:
                    return ActiveContextIsEntityContext(entity) || EntityIsShared(entity);
                case EntityReadAccessLevel.All:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(readAccessLevel), "unsupported read access level");
            }
        }

        public bool AllowCreate<T>()
        {
            return _typeCreationPolicy.AllowCreation(typeof(T));
        }

        public bool AllowCreate<T>(IEntity entity)
        {
            return
                AllowCreate<T>() &&
                CheckNewObjectAccessModifierPolicy(entity) &&
                CheckSpecificCreationPolicy(entity) &&
                AllowModify(entity); //NOTE: Ensures backwards compatibility as long as some terms are yet to be fully migrated
        }

        private EntityReadAccessLevel GetReadAccessLevel(Type entityType)
        {
            var globalRead = _globalReadAccessPolicy.Allow(entityType) || IsGlobalAdmin();
            if (globalRead)
            {
                return EntityReadAccessLevel.All;
            }

            if (IsOrganizationSpecificData(entityType))
            {
                return GetCrossOrganizationReadAccess() >= CrossOrganizationDataReadAccessLevel.Public
                    ? EntityReadAccessLevel.OrganizationAndPublicFromOtherOrganizations
                    : EntityReadAccessLevel.OrganizationOnly;
            }

            return EntityReadAccessLevel.None;
        }

        private bool CheckNewObjectAccessModifierPolicy(IEntity entity)
        {
            if (entity is User user)
            {
                if (user.IsGlobalAdmin)
                {
                    return IsGlobalAdmin(); //Only global admin can create other global admins
                }
            }

            if (entity is IHasAccessModifier accessModifier)
            {
                return HasPermission(new CreateEntityWithVisibilityPermission(accessModifier.AccessModifier, entity));
            }

            return true;
        }

        private bool CheckSpecificCreationPolicy(IEntity entity)
        {
            switch (entity)
            {
                case Organization newOrganization:
                    return CheckOrganizationCreationPolicy(newOrganization);
                case OrganizationRight newOrganizationRight:
                    return AllowAdministerOrganizationRight(newOrganizationRight);
                default:
                    return true;
            }
        }

        private bool CheckOrganizationCreationPolicy(Organization newOrganization)
        {
            var result = true;

            if (newOrganization.TypeId > 0)
            {
                var organizationType = (OrganizationTypeKeys)newOrganization.TypeId;
                if (!HasPermission(new DefineOrganizationTypePermission(organizationType)))
                {
                    result = false;
                }
            }

            return result;
        }

        public bool AllowModify(IEntity entity)
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
            else if (IsOrganizationSpecificData(entity))
            {
                if (ActiveContextIsEntityContext(entity))
                {
                    result =
                        HasModuleLevelWriteAccess(entity) ||
                        HasAssignedWriteAccess(entity);
                }
            }
            else
            {
                result = HasModuleLevelWriteAccess(entity);
            }

            return result;
        }

        public bool AllowDelete(IEntity entity)
        {
            var result = false;
            if (AllowModify(entity))
            {
                switch (entity)
                {
                    case ItSystem _:
                        result = IsGlobalAdmin() || IsLocalAdmin();
                        break;
                    case OrganizationRight right:
                        // Only global admin can set other users as global admins
                        result = AllowAdministerOrganizationRight(right);
                        break;
                    default:
                        result = true;
                        break;
                }
            }

            return result;
        }

        private bool AllowAdministerOrganizationRight(OrganizationRight right)
        {
            return HasPermission(new AdministerOrganizationRightPermission(right));
        }

        public bool HasPermission(Permission permission)
        {
            if (permission == null)
            {
                throw new ArgumentNullException(nameof(permission));
            }

            return permission.Accept(this);
        }

        private bool HasModuleLevelWriteAccess(IEntity entity)
        {
            return _moduleLevelAccessPolicy.AllowModification(entity);
        }

        private bool IsOrganizationModuleAdmin()
        {
            return _activeUserContext.HasRole(OrganizationRole.OrganizationModuleAdmin);
        }

        private static bool IsUserEntity(IEntity entity)
        {
            return entity is User;
        }

        private static bool EntityIsShared(IEntity entity)
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

        private bool IsOrganizationSpecificData(IEntity entity)
        {
            return IsOrganizationSpecificData(_typeResolver.Resolve(entity.GetType()));
        }

        private static bool IsOrganizationSpecificData(Type entityType)
        {
            return typeof(IContextAware).IsAssignableFrom(entityType) || 
                   typeof(IOwnedByOrganization).IsAssignableFrom(entityType) ||
                   typeof(IIsPartOfOrganization).IsAssignableFrom(entityType);
        }

        private bool ActiveContextIsEntityContext(IEntity entity)
        {
            return _activeUserContext.IsActiveInSameOrganizationAs(entity);
        }

        private bool IsGlobalAdmin()
        {
            return _activeUserContext.HasRole(OrganizationRole.GlobalAdmin);
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

        private bool IsContractModuleAdmin()
        {
            return _activeUserContext.HasRole(OrganizationRole.ContractModuleAdmin);
        }

        #region PERMISSIONS
        bool IPermissionVisitor.Visit(BatchImportPermission permission)
        {
            return IsGlobalAdmin() || IsLocalAdmin();
        }

        bool IPermissionVisitor.Visit(SystemUsageMigrationPermission permission)
        {
            return IsGlobalAdmin();
        }

        bool IPermissionVisitor.Visit(VisibilityControlPermission permission)
        {
            var target = permission.Target;
            if (target is IHasAccessModifier)
            {
                switch (target)
                {
                    case IContractModule _:
                        return IsGlobalAdmin() || IsLocalAdmin() || IsContractModuleAdmin();
                    case IOrganizationModule _:
                        return IsGlobalAdmin() || IsLocalAdmin();
                }

                return IsGlobalAdmin();
            }

            //No-one can control access modifiers that are not there
            return false;
        }

        bool IPermissionVisitor.Visit(AdministerOrganizationRightPermission permission)
        {
            var right = permission.Target;

            var result = false;

            if (right.Role == OrganizationRole.GlobalAdmin)
            {
                if (IsGlobalAdmin())
                {
                    result = true;
                }
            }
            // Only local and global admins can make users local admins
            else if (right.Role == OrganizationRole.LocalAdmin)
            {
                if (IsGlobalAdmin() || IsLocalAdmin())
                {
                    result = true;
                }
            }
            else
            {
                result = IsGlobalAdmin() || IsLocalAdmin() || IsOrganizationModuleAdmin();
            }

            return result;
        }

        bool IPermissionVisitor.Visit(DefineOrganizationTypePermission permission)
        {
            switch (permission.TargetOrganizationType)
            {
                case OrganizationTypeKeys.Kommune:
                case OrganizationTypeKeys.AndenOffentligMyndighed:
                    return IsGlobalAdmin();
                case OrganizationTypeKeys.Interessefællesskab:
                case OrganizationTypeKeys.Virksomhed:
                    return IsGlobalAdmin() || IsLocalAdmin();
                default:
                    throw new ArgumentOutOfRangeException(nameof(permission.TargetOrganizationType), permission.TargetOrganizationType, "Unmapped organization type");
            }
        }

        public bool Visit(CreateEntityWithVisibilityPermission permission)
        {
            return permission.Visibility == AccessModifier.Local
                   || (HasPermission(new VisibilityControlPermission(permission.Target)));
        }

        #endregion PERMISSIONS
    }
}