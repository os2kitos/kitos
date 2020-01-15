using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;

namespace Core.ApplicationServices.Authorization
{
    public class ModuleLevelAccessRule : IEntityPolicy
    {
        private readonly IOrganizationalUserContext _userContext;

        public ModuleLevelAccessRule(IOrganizationalUserContext userContext)
        {
            _userContext = userContext;
        }

        public bool Allow(IEntity target)
        {
            var result = IsGlobalAdmin() || IsLocalAdmin();
            switch (target)
            {
                case IContractModule _:
                    result |= IsContractModuleAdmin();
                    break;
                case User _:
                case IOrganizationModule _:
                    result |= IsOrganizationModuleAdmin();
                    break;
                case IProjectModule _:
                    result |= IsProjectModuleAdmin();
                    break;
                case ISystemModule _:
                    result |= IsSystemModuleAdmin();
                    break;
                case IReportModule _:
                    result |= IsReportModuleAdmin();
                    break;
                default:
                    //Unknown module type - no module level access can be granted
                    result = false;
                    break;
            }

            return result;
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
