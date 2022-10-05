using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;
using Core.DomainServices.Organizations;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.Organizations
{
    public class StsOrganizationSynchronizationService : IStsOrganizationSynchronizationService
    {
        private readonly IStsOrganizationUnitService _stsOrganizationUnitService;
        private readonly IOrganizationService _organizationService;
        private readonly ILogger _logger;
        private readonly IStsOrganizationService _stsOrganizationService;
        private readonly IDatabaseControl _databaseControl;
        private readonly ITransactionManager _transactionManager;
        private readonly IAuthorizationContext _authorizationContext;

        public StsOrganizationSynchronizationService(
            IAuthorizationContext authorizationContext,
            IStsOrganizationUnitService stsOrganizationUnitService,
            IOrganizationService organizationService,
            ILogger logger,
            IStsOrganizationService stsOrganizationService,
            IDatabaseControl databaseControl,
            ITransactionManager transactionManager)
        {
            _stsOrganizationUnitService = stsOrganizationUnitService;
            _organizationService = organizationService;
            _logger = logger;
            _stsOrganizationService = stsOrganizationService;
            _databaseControl = databaseControl;
            _transactionManager = transactionManager;
            _authorizationContext = authorizationContext;
        }

        public Result<StsOrganizationSynchronizationDetails, OperationError> GetSynchronizationDetails(Guid organizationId)
        {
            return GetOrganizationWithImportPermission(organizationId)
                .Select(organization =>
                {
                    var currentConnectionStatus = ValidateConnection(organization);
                    var isConnected = organization.StsOrganizationConnection?.Connected == true;
                    var canCreateConnection = currentConnectionStatus.IsNone && organization.StsOrganizationConnection?.Connected != true;
                    var canUpdateConnection = currentConnectionStatus.IsNone && isConnected;
                    return new StsOrganizationSynchronizationDetails
                    (
                        isConnected,
                        organization.StsOrganizationConnection?.SynchronizationDepth,
                        canCreateConnection,
                        canUpdateConnection,
                        isConnected,
                        currentConnectionStatus.Match(error => error.Detail, () => default(CheckConnectionError?))
                    );
                });
        }

        private Maybe<DetailedOperationError<CheckConnectionError>> ValidateConnection(Organization organization)
        {
            return _stsOrganizationService.ValidateConnection(organization);
        }

        public Result<StsOrganizationUnit, OperationError> GetStsOrganizationalHierarchy(Guid organizationId, Maybe<int> levelsToInclude)
        {
            return
                GetOrganizationWithImportPermission(organizationId)
                    .Bind(LoadOrganizationUnits)
                    .Bind(root => FilterByRequestedLevels(root, levelsToInclude));
        }

        public Maybe<OperationError> Connect(Guid organizationId, Maybe<int> levelsToInclude)
        {
            var organizationResult = GetOrganizationWithImportPermission(organizationId);
            if (organizationResult.Failed)
            {
                _logger.Warning("Failed while loading import org ({uuid}) with import permission. {errorCode}:{errorMessage}", organizationId, organizationResult.Error.FailureType, organizationResult.Error.Message.GetValueOrFallback("no-error"));
                return organizationResult.Error;
            }

            //TODO: Move the business logic into the domain part
            var organization = organizationResult.Value;
            if (organization.StsOrganizationConnection?.Connected == true)
            {
                return new OperationError("Already connected", OperationFailure.Conflict);
            }

            return LoadOrganizationUnits(organization)
                .Bind(root => FilterByRequestedLevels(root, levelsToInclude))
                .Match(root => CreateConnection(organization, root, levelsToInclude), error => error);
        }

        private Maybe<OperationError> CreateConnection(Organization organization, StsOrganizationUnit importRoot, Maybe<int> levelsToInclude)
        {
            using var transaction = _transactionManager.Begin();
            //TODO: Move into the domain
            var currentRoot = organization.GetRoot();

            //Switch the origin of the root
            currentRoot.Origin = OrganizationUnitOrigin.STS_Organisation;
            currentRoot.ExternalOriginUuid = importRoot.Uuid;
            currentRoot.Name = importRoot.Name;

            //TODO: Import the sub tree

            organization.StsOrganizationConnection ??= new StsOrganizationConnection();
            organization.StsOrganizationConnection.Connected = true;
            organization.StsOrganizationConnection.SynchronizationDepth = levelsToInclude.Match(levels => (int?)levels, () => default);

            //TODO: Remove - just testing here
            //TODO: This actually works..
            //TODO: Introduce import strategy and change sts org unit to externalOrgUnit
            currentRoot.Children.Add(new OrganizationUnit
            {
                Name = "test",
                Origin = OrganizationUnitOrigin.STS_Organisation,
                ExternalOriginUuid = Guid.NewGuid(),
                Organization = organization,
                Children = new List<OrganizationUnit> { new()
                {
                    Name = "tes2",
                    Origin = OrganizationUnitOrigin.STS_Organisation,
                    ExternalOriginUuid = Guid.NewGuid(),
                    Organization = organization
                } }
            });

            //TODO: We have to first insert the tree, then patch the ids.. we can flatten it and map the parent ids

            transaction.Commit();
            _databaseControl.SaveChanges();

            return Maybe<OperationError>.None;
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

        private static Result<StsOrganizationUnit, OperationError> FilterByRequestedLevels(StsOrganizationUnit root, Maybe<int> levelsToInclude)
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
