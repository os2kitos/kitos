using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    //TODO: Migrate to Permissions and this file is to be PermissionRepository
    public enum Feature
    {
        MakeGlobalAdmin = 1,
        MakeLocalAdmin = 2,
        CanSetAccessModifierToPublic = 3,
        CanSetOrganizationAccessModifierToPublic = 8, //TODO: Replace bu auth check in org service - no need for this feature matrix.
        CanModifyUsers = 9,
        CanModifyContracts = 10,
        CanModifyOrganizations = 11,
        CanModifySystems = 12,
        CanModifyProjects = 13,
        CanModifyReports = 14,
        CanSetContractElementsAccessModifierToPublic = 15
    }

    public class FeatureChecker : IFeatureChecker
    {
        private readonly IOrganizationRoleService _roleService;
        private static readonly IReadOnlyDictionary<Feature, ISet<OrganizationRole>> Features;

        static FeatureChecker()
        {
            Features = new ReadOnlyDictionary<Feature, ISet<OrganizationRole>>(new Dictionary<Feature, ISet<OrganizationRole>>
            {
                {Feature.MakeGlobalAdmin, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin}},
                {Feature.MakeLocalAdmin, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin}},
                {Feature.CanSetAccessModifierToPublic, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin}},
                {Feature.CanSetOrganizationAccessModifierToPublic, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin}},
                {Feature.CanModifyUsers, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin, OrganizationRole.OrganizationModuleAdmin } },
                {Feature.CanModifyContracts, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin, OrganizationRole.ContractModuleAdmin } },
                {Feature.CanModifyOrganizations, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin, OrganizationRole.OrganizationModuleAdmin } },
                {Feature.CanModifyProjects, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin, OrganizationRole.ProjectModuleAdmin } },
                {Feature.CanModifySystems, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin, OrganizationRole.SystemModuleAdmin} },
                {Feature.CanModifyReports, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin, OrganizationRole.ReportModuleAdmin} },
                {Feature.CanSetContractElementsAccessModifierToPublic, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin, OrganizationRole.ContractModuleAdmin} }
            });
        }

        public FeatureChecker(IOrganizationRoleService roleService)
        {
            _roleService = roleService;
        }

        public bool CanExecute(User user, Feature feature)
        {
            var userRoles = CreateRoleList(user);
            if (Features.TryGetValue(feature, out var featureRoles))
            {
                return userRoles.Any(userRole => featureRoles.Contains(userRole));
            }
            return false;
        }

        private IEnumerable<OrganizationRole> CreateRoleList(User user)
        {
            return _roleService.GetRolesInOrganization(user, user.DefaultOrganizationId.GetValueOrDefault());
        }
    }
}