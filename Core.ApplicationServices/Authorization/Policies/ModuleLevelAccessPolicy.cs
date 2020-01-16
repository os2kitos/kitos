using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;

namespace Core.ApplicationServices.Authorization
{
    public class ModuleLevelAccessPolicy : IAuthorizationPolicy<IEntity>
    {
        private readonly IOrganizationalUserContext _userContext;

        public ModuleLevelAccessPolicy(IOrganizationalUserContext userContext)
        {
            _userContext = userContext;
        }

        public bool Allow(IEntity target)
        {
            var possibleConditions = GetPossibleConditions(target).ToList();
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

        private IEnumerable<Func<bool>> GetPossibleConditions(IEntity target)
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
    }
}
