using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization.Policies;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;

namespace Core.ApplicationServices.Authorization
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
            if (target is ItInterface)
            {
                return IsGlobalAdmin();
            }

            if (_onlyGlobalAdminMayEditReports && target is IReportModule)
            {
                return IsGlobalAdmin();
            }
            var possibleConditions = GetPossibleModificationConditions(target).ToList();
            if (!possibleConditions.Any())
            {
                //Unknown module
                return false;
            }

            if (IsGlobalAdmin() || IsLocalAdmin())
            {
                return true;
            }

            return possibleConditions.Any(condition => condition.Invoke());
        }

        private IEnumerable<Func<bool>> GetPossibleModificationConditions(IEntity target)
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
        }

        /// <summary>
        /// Creation policy
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool AllowCreation(Type target)
        {
            if (IsGlobalAdmin())
            {
                return true;
            }

            if (IsReadOnly())
            {
                //Negates all elevated roles
                return false;
            }

            if (MatchType<ItSystem>(target))
            {
                return false; //Only global admin so far
            }

            if (MatchType<ItInterface>(target))
            {
                return false; //Only global admin so far
            }

            if (MatchType<Report>(target) && _onlyGlobalAdminMayEditReports)
            {
                return false;
            }

            //If local admin, all types from this point on are allowed
            if (IsLocalAdmin())
            {
                return true;
            }

            if (MatchType<ItSystemUsage>(target))
            {
                return IsSystemModuleAdmin();
            }

            if (MatchType<ItProject>(target))
            {
                return IsProjectModuleAdmin();
            }

            if (MatchType<ItContract>(target))
            {
                return IsContractModuleAdmin();
            }

            if (MatchType<Organization>(target))
            {
                //Only local admin and global admin may create organizations
                return false;
            }

            if (MatchType<User>(target))
            {
                return IsOrganizationModuleAdmin();
            }

            if (MatchType<Report>(target))
            {
                return IsReportModuleAdmin();
            }

            //NOTE: Other types are yet to be restricted by this policy. In the end a child of e.g. Itsystem should not hit this policy since it is a modification to the root ..> it system
            return true;
        }

        private static bool MatchType<T>(Type type)
        {
            return type == typeof(T);
        }

        private bool IsReportModuleAdmin()
        {
            return _userContext.HasRole(OrganizationRole.ReportModuleAdmin);
        }

        private bool IsSystemModuleAdmin()
        {
            return _userContext.HasRole(OrganizationRole.SystemModuleAdmin);
        }

        private bool IsProjectModuleAdmin()
        {
            return _userContext.HasRole(OrganizationRole.ProjectModuleAdmin);
        }

        private bool IsOrganizationModuleAdmin()
        {
            return _userContext.HasRole(OrganizationRole.OrganizationModuleAdmin);
        }

        private bool IsContractModuleAdmin()
        {
            return _userContext.HasRole(OrganizationRole.ContractModuleAdmin);
        }

        private bool IsLocalAdmin()
        {
            return _userContext.HasRole(OrganizationRole.LocalAdmin);
        }

        private bool IsGlobalAdmin()
        {
            return _userContext.HasRole(OrganizationRole.GlobalAdmin);
        }

        private bool IsReadOnly()
        {
            return _userContext.HasRole(OrganizationRole.ReadOnly);
        }
    }
}
