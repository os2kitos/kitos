using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices
{
    public enum Feature
    {
        MakeGlobalAdmin = 1,
        MakeLocalAdmin,
        CanSetAccessModifierToPublic,
        CanSetOrganizationTypeKommune,
        CanSetOrganizationTypeInteressefællesskab,
        CanSetOrganizationTypeVirksomhed,
        CanSetOrganizationTypeAndenOffentligMyndighed,
        CanSetOrganizationAccessModifierToPublic,
        CanModifyUsers,
        CanModifyContracts,
        CanModifyOrganizations,
        CanModifySystems,
        CanModifyProjects,
        CanModifyReports,
        CanSetContractElementsAccessModifierToPublic
    }

    public class FeatureChecker : IFeatureChecker
    {
        private static readonly IReadOnlyDictionary<Feature, ISet<OrganizationRole>> Features;

        static FeatureChecker()
        {
            Features = new ReadOnlyDictionary<Feature, ISet<OrganizationRole>>(new Dictionary<Feature, ISet<OrganizationRole>>
            {
                {Feature.MakeGlobalAdmin, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin}},
                {Feature.MakeLocalAdmin, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin}},
                {Feature.CanSetOrganizationTypeKommune, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin}},
                {Feature.CanSetOrganizationTypeInteressefællesskab, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin}},
                {Feature.CanSetOrganizationTypeVirksomhed, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin}},
                {Feature.CanSetOrganizationTypeAndenOffentligMyndighed, new HashSet<OrganizationRole> {OrganizationRole.GlobalAdmin}},
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

        public bool CanExecute(User user, Feature feature)
        {
            var userRoles = CreateRoleList(user);
            if (Features.TryGetValue(feature, out var featureRoles))
            {
                return userRoles.Any(userRole => featureRoles.Contains(userRole));
            }
            return false;
        }

        private static IEnumerable<OrganizationRole> CreateRoleList(User user)
        {
            return user.GetRolesInOrg(user.DefaultOrganizationId.GetValueOrDefault());
        }
    }
}