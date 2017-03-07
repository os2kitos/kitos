using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices
{
    public enum Feature
    {
        MakeGlobalAdmin = 1,
        MakeLocalAdmin,
        MakeReportAdmin,
        MakeOrganization,
        CanSetAccessModifierToPublic,
        CanSetOrganizationTypeKommune,
        CanSetOrganizationTypeInteressefællesskab,
        CanSetOrganizationTypeVirksomhed,
        CanSetOrganizationTypeAndenOffentligMyndighed,
        CanSetOrganizationAccessModifierToPublic,
        CanSetContractElementsAccessModifierToPublic
    }

    public class FeatureChecker : IFeatureChecker
    {
        private Dictionary<Feature, List<OrganizationRole>> _features;

        public FeatureChecker()
        {
            Init();
        }

        public bool CanExecute(User user, Feature feature)
        {
            var userRoles = CreateRoleList(user);
            var featureRoles = _features[feature];
            return userRoles.Any(userRole => featureRoles.Contains(userRole));
        }

        private static IEnumerable<OrganizationRole> CreateRoleList(User user)
        {
            var roles = user.OrganizationRights.Select(x => x.Role).Distinct().ToList();
            if (user.IsGlobalAdmin)
                roles.Add(OrganizationRole.GlobalAdmin);

            return roles;
        }

        private void Init()
        {
            _features = new Dictionary<Feature, List<OrganizationRole>>
            {
                {Feature.MakeGlobalAdmin, new List<OrganizationRole> {OrganizationRole.GlobalAdmin}},
                {Feature.MakeLocalAdmin, new List<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin}},
                {Feature.CanSetOrganizationTypeKommune, new List<OrganizationRole> {OrganizationRole.GlobalAdmin}},
                {Feature.CanSetOrganizationTypeInteressefællesskab, new List<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin}},
                {Feature.CanSetOrganizationTypeVirksomhed, new List<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin}},
                {Feature.CanSetOrganizationTypeAndenOffentligMyndighed, new List<OrganizationRole> {OrganizationRole.GlobalAdmin}},
                {Feature.CanSetAccessModifierToPublic, new List<OrganizationRole> {OrganizationRole.GlobalAdmin}},
                {Feature.CanSetOrganizationAccessModifierToPublic, new List<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin}},
                {Feature.CanSetContractElementsAccessModifierToPublic, new List<OrganizationRole> {OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin, OrganizationRole.ContractModuleAdmin} }
            };
        }


    }
}