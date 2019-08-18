using System.Collections.Generic;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;

namespace Presentation.Web.Access
{
    /// <summary>
    /// Determines the user in a specific organizational context
    /// </summary>
    public class OrganizationalUserContext : IOrganizationalUserContext
    {
        private readonly ISet<Feature> _supportedFeatures;
        private readonly ISet<OrganizationRole> _roles;

        public OrganizationalUserContext(
            IEnumerable<Feature> supportedFeatures,
            IEnumerable<OrganizationRole> roles,
            User user, 
            int activeOrganizationId)
        {
            User = user;
            ActiveOrganizationId = activeOrganizationId;
            _supportedFeatures = new HashSet<Feature>(supportedFeatures);
            _roles = new HashSet<OrganizationRole>(roles);
        }

        public User User { get; }
        public int ActiveOrganizationId { get; }

        public bool IsActiveInOrganizationOfType(OrganizationCategory category)
        {
            return User.DefaultOrganization?.Type?.Category == category;
        }

        public bool HasRole(OrganizationRole role)
        {
            return _roles.Contains(role);
        }

        public bool HasModuleLevelAccessTo(IEntity entity)
        {
            var featureToCheck = default(Feature?);
            switch (entity)
            {
                case IContractModule _:
                    featureToCheck = Feature.CanModifyContracts;
                    break;
                case IOrganizationModule _:
                    featureToCheck = Feature.CanModifyOrganizations;
                    break;
                case IProjectModule _:
                    featureToCheck = Feature.CanModifyProjects;
                    break;
                case ISystemModule _:
                    featureToCheck = Feature.CanModifySystems;
                    break;
                case IReportModule _:
                    featureToCheck = Feature.CanModifyReports;
                    break;
                case User _:
                    featureToCheck = Feature.CanModifyUsers;
                    break;
            }

            return featureToCheck.HasValue && _supportedFeatures.Contains(featureToCheck.Value);
        }

        public bool IsActiveInOrganization(int organizationId)
        {
            return User.DefaultOrganizationId == organizationId;
        }

        public bool IsActiveInSameOrganizationAs(IContextAware contextAwareOrg)
        {
            return User.DefaultOrganizationId.HasValue && contextAwareOrg.IsInContext(User.DefaultOrganizationId.Value);
        }
    }
}