using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.KendoConfig;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Authorization.Policies
{
    public class ModuleModificationPolicy :
        IModuleModificationPolicy,
        IModuleCreationPolicy
    {
        private readonly IOrganizationalUserContext _userContext;
        private readonly bool _onlyGlobalAdminMayEditReports;

        public ModuleModificationPolicy(IOrganizationalUserContext userContext, bool onlyGlobalAdminMayEditReports)
        {
            _userContext = userContext;
            _onlyGlobalAdminMayEditReports = onlyGlobalAdminMayEditReports;
        }

        /// <summary>
        /// Modification
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool AllowModification(IEntity target)
        {
            if (target is IHasRightsHolder withRightsHolder)
            {
                var rightsHolderOrganizationId = withRightsHolder.GetRightsHolderOrganizationId();
                if (rightsHolderOrganizationId.Select(IsRightsHolder).GetValueOrFallback(false))
                {
                    // Rights holders bypass the regular rules
                    return true;
                }
            }

            if (_onlyGlobalAdminMayEditReports && target is IReportModule)
            {
                return IsGlobalAdmin();
            }

            var possibleConditions = GetPossibleModificationConditions(target).ToList();

            if (possibleConditions.Any() == false)
            {
                //This target is not subject to module level access
                return false;
            }

            //Global or local admin overrides any of the other conditions
            if (IsGlobalAdmin() || IsLocalAdmin(target))
            {
                return true;
            }

            return possibleConditions.Any(condition => condition.Invoke(target));
        }

        private IEnumerable<Func<IEntity, bool>> GetPossibleModificationConditions(IEntity target)
        {
            //An entity may be marked for several different modules (e.g. Advice), so we must check each
            //Must not be converted to switch since that will only match on the first option. We must check all
            if (target is IContractModule _)
                yield return IsContractModuleAdmin;
            if (target is User _ || target is IOrganizationModule _)
                yield return IsOrganizationModuleAdmin;
            if (target is IProjectModule _)
                yield return IsProjectModuleAdmin;
            if (target is ISystemModule _)
                yield return IsSystemModuleAdmin;
            if (target is IReportModule _)
                yield return IsReportModuleAdmin;
            if (target is IDataProcessingModule _)
            {
                yield return IsSystemModuleAdmin;
                yield return IsContractModuleAdmin;
            }
            if (target is Config _)
                yield return IsLocalAdmin;
            if (target.GetType().IsImplementationOfGenericType(typeof(LocalOptionEntity<>)))
                yield return IsLocalAdmin;
            if (target is KendoOrganizationalConfiguration)
                yield return IsLocalAdmin;
        }

        /// <summary>
        /// Creation policy
        /// </summary>
        /// <returns></returns>
        public bool AllowCreation(int organizationId, Type target)
        {
            if (IsGlobalAdmin())
            {
                return true;
            }

            if (typeof(IHasRightsHolder).IsAssignableFrom(target))
            {
                if (IsRightsHolder(organizationId))
                    return true;

                if (MatchType<ItInterface>(target))
                {
                    return IsSystemModuleAdmin(organizationId) || IsLocalAdmin(organizationId);
                }

                return false;
            }

            if (MatchType<Report>(target) && _onlyGlobalAdminMayEditReports)
            {
                return false;
            }

            //If local admin, all types from this point on are allowed
            if (IsLocalAdmin(organizationId))
            {
                return true;
            }

            if (MatchType<ItSystemUsage>(target))
            {
                return IsSystemModuleAdmin(organizationId);
            }

            if (MatchType<ItProject>(target))
            {
                return IsProjectModuleAdmin(organizationId);
            }

            if (MatchType<ItContract>(target))
            {
                return IsContractModuleAdmin(organizationId);
            }

            if (MatchType<Organization>(target))
            {
                //Only local admin and global admin may create organizations
                return false;
            }

            if (MatchType<User>(target))
            {
                return IsOrganizationModuleAdmin(organizationId);
            }

            if (MatchType<Report>(target))
            {
                return IsReportModuleAdmin(organizationId);
            }

            if (MatchType<DataProcessingRegistration>(target))
            {
                return IsSystemModuleAdmin(organizationId) || IsContractModuleAdmin(organizationId);
            }

            //NOTE: Other types are yet to be restricted by this policy. In the end a child of e.g. Itsystem should not hit this policy since it is a modification to the root ..> it system
            return true;
        }

        private bool IsRightsHolder(int organizationId)
        {
            return _userContext.HasRole(organizationId, OrganizationRole.RightsHolderAccess);
        }

        private bool IsReportModuleAdmin(int organizationId)
        {
            return _userContext.HasRole(organizationId, OrganizationRole.ReportModuleAdmin);
        }

        private bool IsOrganizationModuleAdmin(int organizationId)
        {
            return _userContext.HasRole(organizationId, OrganizationRole.OrganizationModuleAdmin);
        }

        private bool IsContractModuleAdmin(int organizationId)
        {
            return _userContext.HasRole(organizationId, OrganizationRole.ContractModuleAdmin);
        }

        private bool IsProjectModuleAdmin(int organizationId)
        {
            return _userContext.HasRole(organizationId, OrganizationRole.ProjectModuleAdmin);
        }

        private bool IsSystemModuleAdmin(int organizationId)
        {
            return _userContext.HasRole(organizationId, OrganizationRole.SystemModuleAdmin);
        }

        private bool IsLocalAdmin(int organizationId)
        {
            return _userContext.HasRole(organizationId, OrganizationRole.LocalAdmin);
        }

        private static bool MatchType<T>(Type type)
        {
            return type == typeof(T);
        }

        private bool CheckRequiredRoleInRelationTo(IEntity target, OrganizationRole role)
        {
            var organizationIds = new HashSet<int>();

            //If the entity has organizational relationship(s), we check permissions against that
            (target as IIsPartOfOrganization)?.GetOrganizationIds()?.ToList().ForEach(id => organizationIds.Add(id));
            if (target is IOwnedByOrganization ownedByOrganization)
            {
                //If organization is unknown, the object will be fresh and validation must be in the context of the users other rights
                if (ownedByOrganization.Organization != null)
                {
                    organizationIds.Add(ownedByOrganization.Organization.Id);
                }
            }

            if (!organizationIds.Any())
            {
                //Target is not specific to organization so base the check on any role in any of the users organizations
                foreach (var organizationId in _userContext.OrganizationIds)
                {
                    organizationIds.Add(organizationId);
                }
            }

            return organizationIds.Any(id => _userContext.HasRole(id, role));
        }

        private bool IsReportModuleAdmin(IEntity target)
        {
            return CheckRequiredRoleInRelationTo(target, OrganizationRole.ReportModuleAdmin);
        }

        private bool IsSystemModuleAdmin(IEntity target)
        {
            return CheckRequiredRoleInRelationTo(target, OrganizationRole.SystemModuleAdmin);
        }

        private bool IsProjectModuleAdmin(IEntity target)
        {
            return CheckRequiredRoleInRelationTo(target, OrganizationRole.ProjectModuleAdmin);
        }

        private bool IsOrganizationModuleAdmin(IEntity target)
        {
            return CheckRequiredRoleInRelationTo(target, OrganizationRole.OrganizationModuleAdmin);
        }

        private bool IsContractModuleAdmin(IEntity target)
        {
            return CheckRequiredRoleInRelationTo(target, OrganizationRole.ContractModuleAdmin);
        }

        private bool IsLocalAdmin(IEntity target)
        {
            return CheckRequiredRoleInRelationTo(target, OrganizationRole.LocalAdmin);
        }

        private bool IsGlobalAdmin()
        {
            return _userContext.IsGlobalAdmin();
        }
    }
}
