using System;
using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;

namespace Core.ApplicationServices.Authorization
{
    public class OrganizationAuthorizationContext : IAuthorizationContext
    {
        private readonly IOrganizationalUserContext _activeUserContext;

        //NOTE: For types which cannot be bound to a scoped context (lack of knowledge) and has shared read access
        private static readonly IReadOnlyDictionary<Type, bool> TypesWithGlobalReadAccess = new Dictionary<Type, bool>
        {
            {typeof(AdviceUserRelation),true},
            {typeof(Text),true}
    };

        public OrganizationAuthorizationContext(IOrganizationalUserContext activeUserContext)
        {
            _activeUserContext = activeUserContext;
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
            else if (TypesWithGlobalReadAccess.ContainsKey(entity.GetType()))
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
                CheckSpecificCreationPolicy(entity) &&
                AllowModify(entity); //NOTE: Ensures backwards compatibility as long as some terms are yet to be fully migrated
        }

        private bool CheckSpecificCreationPolicy(IEntity entity)
        {
            switch (entity)
            {
                case Organization newOrganization:
                    return CheckNewOrganizationCreationPolicy(newOrganization);
                default:
                    return true;
            }
        }

        private bool CheckNewOrganizationCreationPolicy(Organization newOrganization)
        {
            var result = true;

            if (newOrganization.TypeId > 0)
            {
                var organizationType = (OrganizationTypeKeys) newOrganization.TypeId;
                if (!AllowChangeOrganizationType(organizationType))
                {
                    result = false;
                }
            }

            if (newOrganization.AccessModifier == AccessModifier.Public && !AllowEntityVisibilityControl(newOrganization))
            {
                result = false;
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
            result = result && CheckSpecificTypePolicies(entity);

            //If result is TRUE, this can be negated if read-only is not ignored AND user is marked as read-only
            return result && (ignoreReadOnlyRole || IsReadOnly() == false);
        }

        private bool CheckSpecificTypePolicies(IEntity entity)
        {
            var result = true;

            if (entity is ItInterface)
            {
                //Only global admin can modify interfaces
                result = IsGlobalAdmin();
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
                        result =
                            IsGlobalAdmin() ||
                            (IsLocalAdmin() && ActiveContextIsEntityContext(entity));
                        break;
                    case OrganizationRight right:
                        // Only global admin can set other users as global admins
                        if (right.Role == OrganizationRole.GlobalAdmin)
                        {
                            if (IsGlobalAdmin())
                            {
                                result = true;
                            }
                        }

                        // Only local and global admins can make users local admins
                        if (right.Role == OrganizationRole.LocalAdmin)
                        {
                            if (IsGlobalAdmin() && (IsLocalAdmin() && IsReadOnly() == false))
                            {
                                result = true;
                            }
                        }
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

        public bool AllowSystemUsageMigration()
        {
            return IsGlobalAdmin() && IsReadOnly() == false;
        }

        public bool AllowBatchLocalImport()
        {
            return IsGlobalAdmin() || (IsLocalAdmin() && IsReadOnly() == false);
        }

        public bool AllowChangeOrganizationType(OrganizationTypeKeys organizationType)
        {
            switch (organizationType)
            {
                case OrganizationTypeKeys.Kommune:
                case OrganizationTypeKeys.AndenOffentligMyndighed:
                    return IsGlobalAdmin();
                case OrganizationTypeKeys.Interessefællesskab:
                case OrganizationTypeKeys.Virksomhed:
                    return IsGlobalAdmin() || (IsLocalAdmin() && IsReadOnly() == false);
                default:
                    throw new ArgumentOutOfRangeException(nameof(organizationType), organizationType, "Unmapped organization type");
            }
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
    }
}