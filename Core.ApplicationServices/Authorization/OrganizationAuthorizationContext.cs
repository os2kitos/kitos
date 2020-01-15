using System;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;

namespace Core.ApplicationServices.Authorization
{
    public class OrganizationAuthorizationContext : IAuthorizationContext, IPermissionVisitor
    {
        private readonly IOrganizationalUserContext _activeUserContext;
        private readonly IEntityPolicy _moduleLevelAccessPolicy;
        private readonly IEntityPolicy _globalReadAccessPolicy;

        public OrganizationAuthorizationContext(
            IOrganizationalUserContext activeUserContext,
            IEntityPolicy moduleLevelAccessPolicy,
            IEntityPolicy globalReadAccessPolicy)
        {
            _activeUserContext = activeUserContext;
            _moduleLevelAccessPolicy = moduleLevelAccessPolicy;
            _globalReadAccessPolicy = globalReadAccessPolicy;
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
                else if (GetCrossOrganizationReadAccess() >= CrossOrganizationDataReadAccessLevel.Public && EntityIsShared(entity))
                {
                    result = true;
                }
            }
            else if (_globalReadAccessPolicy.Allow(entity))
            {
                result = true;
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
                CheckNewObjectAccessModifierPolicy(entity) &&
                CheckSpecificCreationPolicy(entity) &&
                AllowModify(entity); //NOTE: Ensures backwards compatibility as long as some terms are yet to be fully migrated
        }

        private bool CheckNewObjectAccessModifierPolicy(IEntity entity)
        {
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

            var ignoreReadOnlyRole = false;

            if (IsGlobalAdmin())
            {
                ignoreReadOnlyRole = true; //Global admin cannot be locally read-ony
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

            //Specific type policy may revoke existing result so AND it
            result = result && CheckSpecificTypeModificationPolicies(entity);

            //If result is TRUE, this can be negated if read-only is not ignored AND user is marked as read-only
            return result && (ignoreReadOnlyRole || IsReadOnly() == false);
        }

        private bool CheckSpecificTypeModificationPolicies(IEntity entity)
        {
            var result = true;

            switch (entity)
            {
                case ItInterface _:
                    result = IsGlobalAdmin();
                    break;
            }

            return result || IsGlobalAdmin();
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
            return _moduleLevelAccessPolicy.Allow(entity);
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

        private bool IsContractModuleAdmin()
        {
            return _activeUserContext.HasRole(OrganizationRole.ContractModuleAdmin);
        }

        #region PERMISSIONS
        bool IPermissionVisitor.Visit(BatchImportPermission permission)
        {
            return IsGlobalAdmin() || (IsLocalAdmin() && IsReadOnly() == false);
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
                        return IsGlobalAdmin() || ((IsLocalAdmin() || IsContractModuleAdmin()) && IsReadOnly() == false);
                    case IOrganizationModule _:
                        return IsGlobalAdmin() || (IsLocalAdmin() && IsReadOnly() == false);
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
                if (IsGlobalAdmin() || (IsLocalAdmin() && IsReadOnly() == false))
                {
                    result = true;
                }
            }
            else
            {
                result = IsGlobalAdmin() || ((IsLocalAdmin() || IsOrganizationModuleAdmin()) && IsReadOnly() == false);
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
                    return IsGlobalAdmin() || (IsLocalAdmin() && IsReadOnly() == false);
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