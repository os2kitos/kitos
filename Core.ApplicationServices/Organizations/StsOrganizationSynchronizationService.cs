using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;
using Core.DomainServices.Organizations;

namespace Core.ApplicationServices.Organizations
{
    public class StsOrganizationSynchronizationService : IStsOrganizationSynchronizationService
    {
        private readonly IStsOrganizationUnitService _stsOrganizationUnitService;
        private readonly IOrganizationService _organizationService;
        private readonly IAuthorizationContext _authorizationContext;

        public StsOrganizationSynchronizationService(
            IAuthorizationContext authorizationContext,
            IStsOrganizationUnitService stsOrganizationUnitService,
            IOrganizationService organizationService)
        {
            _stsOrganizationUnitService = stsOrganizationUnitService;
            _organizationService = organizationService;
            _authorizationContext = authorizationContext;
        }

        public Result<StsOrganizationUnit, OperationError> GetStsOrganizationalHierarchy(Guid organizationId, Maybe<uint> levelsToInclude)
        {
            return _organizationService
                .GetOrganization(organizationId)
                .Bind(WithImportPermission)
                .Bind(_stsOrganizationUnitService.ResolveOrganizationTree)
                .Bind(root => FilterByRequestedLevels(root, levelsToInclude));
        }

        private Result<Organization, OperationError> WithImportPermission(Organization organization)
        {
            if (_authorizationContext.HasPermission(new ImportHierarchyFromStsOrganizationPermission(organization)))
            {
                return organization;
            }
            return new OperationError($"The user does not have permission to use the STS Organization Sync functionality for the organization with uuid:{organization.Uuid}", OperationFailure.Forbidden);
        }

        private Result<StsOrganizationUnit, OperationError> FilterByRequestedLevels(StsOrganizationUnit root, Maybe<uint> levelsToInclude)
        {
            if (levelsToInclude.IsNone)
            {
                return root;
            }

            var levels = levelsToInclude.Value;
            if (levels < 1)
            {
                return new OperationError($"{nameof(levelsToInclude)} must be greater than or equal to 1", OperationFailure.BadInput);
            }

            levels--;
            return CopyStsOrganizationUnitWithFiltering(root, levels);
        }

        private StsOrganizationUnit CopyStsOrganizationUnitWithFiltering(StsOrganizationUnit unit, uint levels)
        {
            return new StsOrganizationUnit(unit.Uuid, unit.Name, unit.UserFacingKey, GetChildren(unit.Children, levels).ToList());
        }

        private IEnumerable<StsOrganizationUnit> GetChildren(IEnumerable<StsOrganizationUnit> currentChildren, uint levelsLeft)
        {
            if (levelsLeft < 1) yield break;

            levelsLeft--;
            foreach (var currentChild in currentChildren)
            {
                yield return CopyStsOrganizationUnitWithFiltering(currentChild, levelsLeft);
            }
        }
    }
}
