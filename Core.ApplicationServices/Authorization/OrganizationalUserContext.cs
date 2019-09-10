using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;

namespace Core.ApplicationServices.Authorization
{
    /// <summary>
    /// Determines the user in a specific organizational context
    /// </summary>
    public class OrganizationalUserContext : IOrganizationalUserContext
    {
        private readonly ISet<Feature> _supportedFeatures;
        private readonly ISet<OrganizationRole> _roles;
        private readonly User _user;

        public OrganizationalUserContext(
            IEnumerable<Feature> supportedFeatures,
            IEnumerable<OrganizationRole> roles,
            User user,
            int activeOrganizationId)
        {
            _user = user;
            ActiveOrganizationId = activeOrganizationId;
            _supportedFeatures = new HashSet<Feature>(supportedFeatures);
            _roles = new HashSet<OrganizationRole>(roles);
        }

        public int ActiveOrganizationId { get; }

        public int UserId => _user.Id;

        public bool IsActiveInOrganizationOfType(OrganizationCategory category)
        {
            return _user.DefaultOrganization?.Type?.Category == category;
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
            return ActiveOrganizationId == organizationId;
        }

        public bool IsActiveInSameOrganizationAs(IEntity entity)
        {
            switch (entity)
            {
                case IContextAware contextAware:
                    return contextAware.IsInContext(ActiveOrganizationId);
                case IHasOrganization hasOrg:
                    return IsActiveInOrganization(hasOrg.OrganizationId);
                default:
                    return false;
            }
        }

        public bool HasAssignedWriteAccess(IEntity entity)
        {
            return entity.HasUserWriteAccess(_user);
        }

        public bool HasOwnership(IEntity entity)
        {
            return entity.ObjectOwnerId == UserId;
        }

        public bool CanChangeVisibilityOf(IEntity entity)
        {
            if (entity is IHasAccessModifier)
            {
                switch (entity)
                {
                    case IContractModule _:
                        return _supportedFeatures.Contains(Feature.CanSetContractElementsAccessModifierToPublic);
                    case IOrganizationModule _:
                        return _supportedFeatures.Contains(Feature.CanSetOrganizationAccessModifierToPublic);
                }

                return _supportedFeatures.Contains(Feature.CanSetAccessModifierToPublic);
            }

            return false;
        }
    }
}