using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;
using Core.DomainServices.Organizations;
using Serilog;

namespace Core.ApplicationServices.Organizations
{
    public class StsOrganizationSynchronizationService : IStsOrganizationSynchronizationService
    {
        private readonly IStsOrganizationUnitService _stsOrganizationUnitService;
        private readonly IOrganizationService _organizationService;
        private readonly ILogger _logger;
        private readonly IAuthorizationContext _authorizationContext;

        public StsOrganizationSynchronizationService(
            IAuthorizationContext authorizationContext,
            IStsOrganizationUnitService stsOrganizationUnitService,
            IOrganizationService organizationService,
            ILogger logger)
        {
            _stsOrganizationUnitService = stsOrganizationUnitService;
            _organizationService = organizationService;
            _logger = logger;
            _authorizationContext = authorizationContext;
        }

        public Maybe<OperationError> ValidateConnection(Guid organizationId)
        {
            return GetOrganizationWithImportPermission(organizationId)
                .Match(ValidateConnection, error => error);
        }

        private Maybe<OperationError> ValidateConnection(Organization organization)
        {
            return _stsOrganizationUnitService.ValidateConnection(organization);
        }

        public Result<StsOrganizationUnit, OperationError> GetStsOrganizationalHierarchy(Guid organizationId, Maybe<uint> levelsToInclude)
        {
            return
                GetOrganizationWithImportPermission(organizationId)
                    .Bind(LoadOrganizationUnits)
                    .Bind(root => FilterByRequestedLevels(root, levelsToInclude));
        }

        private Result<StsOrganizationUnit, OperationError> LoadOrganizationUnits(Organization organization)
        {
            return _stsOrganizationUnitService.ResolveOrganizationTree(organization).Match<Result<StsOrganizationUnit, OperationError>>(root => root, detailedOperationError => new OperationError($"Failed to load organization tree:{detailedOperationError.Detail:G}:{detailedOperationError.FailureType:G}:{detailedOperationError.Message}", detailedOperationError.FailureType));
        }

        private Result<Organization, OperationError> GetOrganizationWithImportPermission(Guid organizationId)
        {
            return _organizationService
                .GetOrganization(organizationId)
                .Bind(WithImportPermission);
        }

        private Result<Organization, OperationError> WithImportPermission(Organization organization)
        {
            if (_authorizationContext.HasPermission(new ImportHierarchyFromStsOrganizationPermission(organization)))
            {
                return organization;
            }
            return new OperationError($"The user does not have permission to use the STS Organization Sync functionality for the organization with uuid:{organization.Uuid}", OperationFailure.Forbidden);
        }

        private static Result<StsOrganizationUnit, OperationError> FilterByRequestedLevels(StsOrganizationUnit root, Maybe<uint> levelsToInclude)
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
            return root.Copy(levels);
        }
    }
}
