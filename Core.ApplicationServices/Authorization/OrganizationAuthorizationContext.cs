using System;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Authorization.Policies;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
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
        private readonly IUserRepository _userRepository;

        public OrganizationAuthorizationContext(
            IOrganizationalUserContext activeUserContext,
            IEntityTypeResolver typeResolver,
            IModuleModificationPolicy moduleLevelAccessPolicy,
            IGlobalReadAccessPolicy globalReadAccessPolicy,
            IModuleCreationPolicy typeCreationPolicy,
            IUserRepository userRepository)
        {
            _activeUserContext = activeUserContext;
            _typeResolver = typeResolver;
            _moduleLevelAccessPolicy = moduleLevelAccessPolicy;
            _globalReadAccessPolicy = globalReadAccessPolicy;
            _typeCreationPolicy = typeCreationPolicy;
            _userRepository = userRepository;
        }

        public CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccess()
        {
            if (IsGlobalAdmin())
            {
                return CrossOrganizationDataReadAccessLevel.All;
            }

            //If rightsholder access is selected for the user it overrides the default calculation
            if (_activeUserContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess))
                return CrossOrganizationDataReadAccessLevel.RightsHolder;

            return (IsUserInMunicipality() || HasStakeHolderAccess())
                ? CrossOrganizationDataReadAccessLevel.Public
                : CrossOrganizationDataReadAccessLevel.None;
        }

        public OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId)
        {
            if (HasRoleIn(organizationId))
            {
                return OrganizationDataReadAccessLevel.All;
            }

            return GetCrossOrganizationReadAccess() switch
            {
                CrossOrganizationDataReadAccessLevel.Public => OrganizationDataReadAccessLevel.Public,
                CrossOrganizationDataReadAccessLevel.All => OrganizationDataReadAccessLevel.All,
                CrossOrganizationDataReadAccessLevel.RightsHolder => OrganizationDataReadAccessLevel.RightsHolder,
                _ => OrganizationDataReadAccessLevel.None
            };
        }

        private bool HasRightsHolderAccessIn(int organizationId)
        {
            return _activeUserContext.HasRole(organizationId, OrganizationRole.RightsHolderAccess);
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
            return readAccessLevel switch
            {
                EntityReadAccessLevel.None => false,
                EntityReadAccessLevel.OrganizationAndRightsHolderAccess =>
                    HasRoleInSameOrganizationAs(entity) ||
                    IsRightsHolderFor(entity),
                EntityReadAccessLevel.OrganizationOnly => HasRoleInSameOrganizationAs(entity),
                EntityReadAccessLevel.OrganizationAndPublicFromOtherOrganizations =>
                    HasRoleInSameOrganizationAs(entity) ||
                    EntityIsShared(entity),
                EntityReadAccessLevel.All => true,
                _ => throw new ArgumentOutOfRangeException(nameof(readAccessLevel), "unsupported read access level")
            };
        }

        private bool IsRightsHolderFor(IEntity entity)
        {
            var result = false;

            if (entity is IHasRightsHolder withRightsHolder)
            {
                //Check 1: By registered rights holder (allows access in other organizations than the one where the user has access by normal rules)
                result = withRightsHolder
                    .GetRightsHolderOrganizationId()
                    .Select(HasRightsHolderAccessIn)
                    .GetValueOrFallback(false);

                //Check 2: If the object is created in the rightsholder's own organization - in that case rights holder access will provide access to that object the rights holders own organization
                if (!result)
                {
                    if (entity is IOwnedByOrganization withOrganization)
                    {
                        result = HasRightsHolderAccessIn(withOrganization.OrganizationId);
                    }
                }
            }

            return result;
        }

        public bool AllowCreate<T>(int organizationId)
        {
            return _typeCreationPolicy.AllowCreation(organizationId, typeof(T));
        }

        public bool AllowCreate<T>(int organizationId, IEntity entity)
        {
            return
                AllowCreate<T>(organizationId) &&
                CheckNewObjectAccessModifierPolicy(organizationId, entity) &&
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
                var crossOrganizationDataReadAccessLevel = GetCrossOrganizationReadAccess();

                return crossOrganizationDataReadAccessLevel switch
                {
                    CrossOrganizationDataReadAccessLevel.RightsHolder => EntityReadAccessLevel.OrganizationAndRightsHolderAccess,
                    CrossOrganizationDataReadAccessLevel.All => EntityReadAccessLevel.OrganizationAndPublicFromOtherOrganizations,
                    CrossOrganizationDataReadAccessLevel.Public => EntityReadAccessLevel.OrganizationAndPublicFromOtherOrganizations,
                    CrossOrganizationDataReadAccessLevel.None => EntityReadAccessLevel.OrganizationOnly,
                    _ => EntityReadAccessLevel.OrganizationOnly
                };
            }

            return EntityReadAccessLevel.None;
        }

        private bool CheckNewObjectAccessModifierPolicy(int organizationId, IEntity entity)
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
                return HasPermission(new CreateEntityWithVisibilityPermission(accessModifier.AccessModifier, entity, organizationId));
            }

            return true;
        }

        private bool CheckSpecificCreationPolicy(IEntity entity)
        {
            switch (entity)
            {
                case OrganizationRight newOrganizationRight:
                    return AllowAdministerOrganizationRight(newOrganizationRight);
                default:
                    return true;
            }
        }

        public bool AllowModify(IEntity entity)
        {
            var result = false;

            if (IsGlobalAdmin())
            {
                result = true;
            }
            else if (entity is OrganizationRight right)
            {
                result = AllowAdministerOrganizationRight(right);
            }
            else if (EntityEqualsActiveUser(entity))
            {
                result = true;
            }
            else if (IsRightsHolderFor(entity))
            {
                result = true;
            }
            else if (IsOrganizationSpecificData(entity))
            {
                if (HasRoleInSameOrganizationAs(entity))
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
                result = entity switch
                {
                    User user => IsGlobalAdmin() && EntityEqualsActiveUser(user) == false,
                    ItInterface itInterface =>
                        //Even rightsholders are not allowed to delete interfaces
                        IsGlobalAdmin() || IsLocalAdmin(itInterface.OrganizationId),
                    ItSystem system =>
                        //Even rightsholders are not allowed to delete systems
                        IsGlobalAdmin() || IsLocalAdmin(system.OrganizationId),
                    OrganizationRight right =>
                        // Only global admin can set other users as global admins
                        AllowAdministerOrganizationRight(right),
                    OrganizationUnit unit =>
                        IsGlobalAdmin() || IsLocalAdmin(unit.OrganizationId),
                    _ => true
                };
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

        private bool IsOrganizationModuleAdmin(int organizationId)
        {
            return _activeUserContext.HasRole(organizationId, OrganizationRole.OrganizationModuleAdmin);
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
            return _activeUserContext.HasRoleInOrganizationOfType(OrganizationCategory.Municipality);
        }

        private bool HasRoleIn(int targetOrganizationId)
        {
            return _activeUserContext.HasRoleIn(targetOrganizationId);
        }

        private bool HasAssignedWriteAccess(IEntity entity)
        {
            if (entity is ISupportsUserSpecificAccessControl withUserAccessControl)
            {
                return _userRepository
                .GetById(_activeUserContext.UserId)
                .FromNullable()
                .Select(withUserAccessControl.HasUserWriteAccess)
                .GetValueOrFallback(false);

            }

            return false;
        }

        private bool IsOrganizationSpecificData(IEntity entity)
        {
            return IsOrganizationSpecificData(_typeResolver.Resolve(entity.GetType()));
        }

        private static bool IsOrganizationSpecificData(Type entityType)
        {
            return typeof(IOwnedByOrganization).IsAssignableFrom(entityType) ||
                   typeof(IIsPartOfOrganization).IsAssignableFrom(entityType);
        }

        private bool HasStakeHolderAccess()
        {
            return _activeUserContext.HasStakeHolderAccess();
        }

        private bool HasRoleInSameOrganizationAs(IEntity entity)
        {
            return _activeUserContext.HasRoleInSameOrganizationAs(entity);
        }

        private bool IsGlobalAdmin()
        {
            return _activeUserContext.IsGlobalAdmin();
        }

        private bool IsLocalAdmin(int organizationId)
        {
            return _activeUserContext.HasRole(organizationId, OrganizationRole.LocalAdmin);
        }

        private bool EntityEqualsActiveUser(IEntity entity)
        {
            return IsUserEntity(entity) && entity.Id == _activeUserContext.UserId;
        }

        private bool IsContractModuleAdmin(int organizationId)
        {
            return _activeUserContext.HasRole(organizationId, OrganizationRole.ContractModuleAdmin);
        }

        #region PERMISSIONS
        bool IPermissionVisitor.Visit(BatchImportPermission permission)
        {
            return IsGlobalAdmin() || IsLocalAdmin(permission.TargetOrganizationId);
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
                var ownedByOrganization = (IOwnedByOrganization)target;

                return target switch
                {
                    IContractModule _ => IsGlobalAdmin() ||
                                         IsLocalAdmin(ownedByOrganization.OrganizationId) ||
                                         IsContractModuleAdmin(ownedByOrganization.OrganizationId),
                    IOrganizationModule _ => IsGlobalAdmin() ||
                                             IsLocalAdmin(ownedByOrganization.OrganizationId),
                    _ => IsGlobalAdmin()
                };
            }

            //No-one can control access modifiers that are not there
            return false;
        }

        bool IPermissionVisitor.Visit(AdministerOrganizationRightPermission permission)
        {
            var right = permission.Target;

            var isGlobalAdmin = IsGlobalAdmin();
            var hasFullLocalAccess = isGlobalAdmin || IsLocalAdmin(right.OrganizationId);

            return right.Role switch
            {
                OrganizationRole.GlobalAdmin => HasPermission(new AdministerGlobalPermission(GlobalPermission.GlobalAdmin)),
                OrganizationRole.RightsHolderAccess => isGlobalAdmin,
                OrganizationRole.LocalAdmin => hasFullLocalAccess,
                OrganizationRole.User => hasFullLocalAccess || IsOrganizationModuleAdmin(right.OrganizationId),
                OrganizationRole.OrganizationModuleAdmin => hasFullLocalAccess || IsOrganizationModuleAdmin(right.OrganizationId),
                _ => hasFullLocalAccess
            };
        }

        bool IPermissionVisitor.Visit(DefineOrganizationTypePermission permission)
        {
            return permission.TargetOrganizationType switch
            {
                OrganizationTypeKeys.Kommune => IsGlobalAdmin(),
                OrganizationTypeKeys.AndenOffentligMyndighed => IsGlobalAdmin(),
                OrganizationTypeKeys.Interessefællesskab => IsGlobalAdmin() || IsLocalAdmin(permission.ParentOrganizationId),
                OrganizationTypeKeys.Virksomhed => IsGlobalAdmin() || IsLocalAdmin(permission.ParentOrganizationId),
                _ => throw new ArgumentOutOfRangeException(nameof(permission.TargetOrganizationType), permission.TargetOrganizationType, "Unmapped organization type")
            };
        }

        public bool Visit(CreateEntityWithVisibilityPermission permission)
        {
            return permission.Visibility == AccessModifier.Local
                   || (HasPermission(new VisibilityControlPermission(permission.Target)));
        }

        public bool Visit(ViewBrokenExternalReferencesReportPermission permission)
        {
            return IsGlobalAdmin();
        }

        public bool Visit(TriggerBrokenReferencesReportPermission permission)
        {
            return IsGlobalAdmin();
        }

        public bool Visit(AdministerGlobalPermission permission)
        {
            return permission.Permission switch
            {
                GlobalPermission.GlobalAdmin => IsGlobalAdmin(),
                GlobalPermission.StakeHolderAccess => IsGlobalAdmin(),
                _ => false
            };
        }

        public bool Visit(ImportHierarchyFromStsOrganizationPermission permission)
        {
            var organizationId = permission.Organization.Id;
            return IsGlobalAdmin() ||
                   IsLocalAdmin(organizationId) ||
                   IsOrganizationModuleAdmin(organizationId);
        }

        public bool Visit(BulkAdministerOrganizationUnitRegistrations permission)
        {
            var organizationId = permission.OrganizationId;
            return IsGlobalAdmin() || IsLocalAdmin(organizationId);
        }

        public bool Visit(DeleteAnyUserPermission permission)
        {
            var organization = permission.OptionalOrganizationScopeId;
            return IsGlobalAdmin() || organization.Select(IsLocalAdmin).GetValueOrFallback(false);
        }

        public bool Visit(ChangeLegalSystemPropertiesPermission permission)
        {
            return _activeUserContext.IsSystemIntegrator();
        }

        #endregion PERMISSIONS
    }
}