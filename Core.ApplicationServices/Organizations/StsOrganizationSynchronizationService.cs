using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Model.StsOrganization;
using Core.DomainServices.Organizations;
using Infrastructure.Services.DataAccess;
using Serilog;
using Organization = Core.DomainModel.Organization.Organization;

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
        private readonly IDomainEvents _domainEvents;
        private readonly IGenericRepository<OrganizationUnit> _organizationUnitRepository;
        private readonly IAuthorizationContext _authorizationContext;

        public StsOrganizationSynchronizationService(
            IAuthorizationContext authorizationContext,
            IStsOrganizationUnitService stsOrganizationUnitService,
            IOrganizationService organizationService,
            ILogger logger,
            IStsOrganizationService stsOrganizationService,
            IDatabaseControl databaseControl,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IGenericRepository<OrganizationUnit> organizationUnitRepository)
        {
            _stsOrganizationUnitService = stsOrganizationUnitService;
            _organizationService = organizationService;
            _logger = logger;
            _stsOrganizationService = stsOrganizationService;
            _databaseControl = databaseControl;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _organizationUnitRepository = organizationUnitRepository;
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

        public Result<ExternalOrganizationUnit, OperationError> GetStsOrganizationalHierarchy(Guid organizationId, Maybe<int> levelsToInclude)
        {
            return
                GetOrganizationWithImportPermission(organizationId)
                    .Bind(LoadOrganizationUnits)
                    .Bind(root => FilterByRequestedLevels(root, levelsToInclude));
        }

        public Maybe<OperationError> Connect(Guid organizationId, Maybe<int> levelsToInclude, bool subscribeToUpdates)
        {
            return Modify(organizationId, organization =>
            {
                return LoadOrganizationUnits(organization)
                    .Match
                    (
                        importRoot =>
                        {
                            var error = organization.ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, importRoot, levelsToInclude, subscribeToUpdates);
                            if (error.HasValue)
                            {
                                _logger.Error("Failed to import org root {rootId} and subtree into organization with id {orgId}. Failed with: {errorCode}:{errorMessage}", importRoot.Uuid, organization.Id, error.Value.FailureType, error.Value.Message.GetValueOrFallback(""));
                                return new OperationError("Failed to import sub tree", OperationFailure.UnknownError);
                            }

                            return Maybe<OperationError>.None;
                        },
                        error => error
                    );
            });
        }

        public Maybe<OperationError> Disconnect(Guid organizationId)
        {
            return Modify(organizationId, organization =>
            {
                var result = organization.DisconnectOrganizationFromExternalSource(OrganizationUnitOrigin.STS_Organisation);
                if (result.Failed)
                {
                    return result.Error;
                }

                var disconnectionResult = result.Value;
                foreach (var convertedUnit in disconnectionResult.ConvertedUnits)
                {
                    _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(convertedUnit));
                }
                return Maybe<OperationError>.None;
            });
        }

        public Result<OrganizationTreeUpdateConsequences, OperationError> GetConnectionExternalHierarchyUpdateConsequences(Guid organizationId, Maybe<int> levelsToInclude)
        {
            return GetOrganizationWithImportPermission(organizationId)
                .Match(organization =>
                        LoadOrganizationUnits(organization)
                            .Bind(root => organization
                                .ComputeExternalOrganizationHierarchyUpdateConsequences(
                                    OrganizationUnitOrigin.STS_Organisation, root, levelsToInclude))
                    ,
                    error => error
                );
        }

        public Maybe<OperationError> UpdateConnection(Guid organizationId, Maybe<int> levelsToInclude, bool subscribeToUpdates)
        {
            return Modify(organizationId, organization =>
                LoadOrganizationUnits(organization)
                    .Bind(importRoot => organization.UpdateConnectionToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, importRoot, levelsToInclude, subscribeToUpdates))
                    .Select(consequences =>
                    {
                        if (consequences.DeletedExternalUnitsBeingDeleted.Any())
                        {
                            _organizationUnitRepository.RemoveRange(consequences.DeletedExternalUnitsBeingDeleted);
                        }
                        foreach (var (affectedUnit, _, _) in consequences.OrganizationUnitsBeingRenamed)
                        {
                            _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(affectedUnit));
                        }
                        return consequences;
                    })
                    .Match(_ => Maybe<OperationError>.None, error => error)
            );
        }


        private Result<ExternalOrganizationUnit, OperationError> LoadOrganizationUnits(Organization organization)
        {
            return _stsOrganizationUnitService.ResolveOrganizationTree(organization).Match<Result<ExternalOrganizationUnit, OperationError>>(root => root, detailedOperationError => new OperationError($"Failed to load organization tree:{detailedOperationError.Detail:G}:{detailedOperationError.FailureType:G}:{detailedOperationError.Message}", detailedOperationError.FailureType));
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

        private static Result<ExternalOrganizationUnit, OperationError> FilterByRequestedLevels(ExternalOrganizationUnit root, Maybe<int> levelsToInclude)
        {
            if (levelsToInclude.IsNone)
            {
                return root;
            }

            if (levelsToInclude.Value < 1)
            {
                return new OperationError($"{nameof(levelsToInclude)} must be greater than or equal to 1", OperationFailure.BadInput);
            }

            return root.Copy(levelsToInclude.Select(levels => levels - 1));
        }

        private Maybe<OperationError> Modify(Guid organizationUuid, Func<Organization, Maybe<OperationError>> mutate)
        {
            using var transaction = _transactionManager.Begin();

            var organizationResult = GetOrganizationWithImportPermission(organizationUuid);
            if (organizationResult.Failed)
            {
                _logger.Warning("Failed while loading import org ({uuid}) with import permission. {errorCode}:{errorMessage}", organizationUuid, organizationResult.Error.FailureType, organizationResult.Error.Message.GetValueOrFallback("no-error"));
                return organizationResult.Error;
            }

            var organization = organizationResult.Value;
            var mutationError = mutate(organization);
            if (mutationError.HasValue)
            {
                transaction.Rollback();
                return mutationError;
            }

            _domainEvents.Raise(new EntityUpdatedEvent<Organization>(organization));
            _databaseControl.SaveChanges();
            transaction.Commit();

            return Maybe<OperationError>.None;
        }
    }
}
