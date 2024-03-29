﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Commands;
using Core.DomainModel.Events;
using Core.DomainModel.Extensions;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Context;
using Core.DomainServices.Model.StsOrganization;
using Core.DomainServices.Organizations;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.Organizations
{
    public class StsOrganizationSynchronizationService : IStsOrganizationSynchronizationService
    {
        private readonly IStsOrganizationSystemService _stsOrganizationSystemService;
        private readonly IOrganizationService _organizationService;
        private readonly ILogger _logger;
        private readonly IStsOrganizationService _stsOrganizationService;
        private readonly IDatabaseControl _databaseControl;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly Maybe<ActiveUserIdContext> _activeUserIdContext;
        private readonly IGenericRepository<StsOrganizationChangeLog> _stsChangeLogRepository;
        private readonly IOperationClock _operationClock;
        private readonly ICommandBus _commandBus;

        public StsOrganizationSynchronizationService(
            IAuthorizationContext authorizationContext,
            IStsOrganizationSystemService stsOrganizationSystemService,
            IOrganizationService organizationService,
            ILogger logger,
            IStsOrganizationService stsOrganizationService,
            IDatabaseControl databaseControl,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            Maybe<ActiveUserIdContext> activeUserIdContext,
            IGenericRepository<StsOrganizationChangeLog> stsChangeLogRepository,
            IOperationClock operationClock,
            ICommandBus commandBus)
        {
            _stsOrganizationSystemService = stsOrganizationSystemService;
            _organizationService = organizationService;
            _logger = logger;
            _stsOrganizationService = stsOrganizationService;
            _databaseControl = databaseControl;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _authorizationContext = authorizationContext;
            _activeUserIdContext = activeUserIdContext;
            _stsChangeLogRepository = stsChangeLogRepository;
            _operationClock = operationClock;
            _commandBus = commandBus;
        }

        public Result<StsOrganizationSynchronizationDetails, OperationError> GetSynchronizationDetails(Guid organizationId)
        {
            return GetOrganizationWithImportPermission(organizationId)
                .Select(organization =>
                {
                    var currentConnectionStatus = ValidateConnection(organization);
                    var isConnected = organization.StsOrganizationConnection?.Connected == true;
                    var subscribesToUpdates = organization.StsOrganizationConnection?.SubscribeToUpdates == true;
                    var dateOfLatestCheckBySubscription = organization.StsOrganizationConnection?.DateOfLatestCheckBySubscription;
                    var canCreateConnection = currentConnectionStatus.IsNone && organization.StsOrganizationConnection?.Connected != true;
                    var canUpdateConnection = currentConnectionStatus.IsNone && isConnected;
                    return new StsOrganizationSynchronizationDetails
                    (
                        isConnected,
                        organization.StsOrganizationConnection?.SynchronizationDepth,
                        canCreateConnection,
                        canUpdateConnection,
                        isConnected,
                        currentConnectionStatus.Match(error => error.Detail, () => default(CheckConnectionError?)),
                        subscribesToUpdates,
                        dateOfLatestCheckBySubscription
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
                    .Bind(importRoot => ConnectToExternalOrganizationHierarchy(organization, importRoot, levelsToInclude, subscribeToUpdates))
                    .Select(ToConnectionConsequences)
                    .Select(x => x.ToLogEntries(_activeUserIdContext, _operationClock.Now))
                    .Bind(logEntries => organization.AddExternalImportLog(OrganizationUnitOrigin.STS_Organisation, logEntries))
                    .MatchFailure();
            });
        }

        public Maybe<OperationError> Disconnect(Guid organizationId, bool purgeUnusedExternalOrganizationUnits)
        {
            return Modify(organizationId, organization =>
            {
                if (purgeUnusedExternalOrganizationUnits)
                {
                    //Perform sync to level 1 before disconnecting and let the update functionality deal with the consequence calculations
                    var purgeError = _commandBus.Execute<AuthorizedUpdateOrganizationFromFKOrganisationCommand, Maybe<OperationError>>(new AuthorizedUpdateOrganizationFromFKOrganisationCommand(organization, 1, false, ToExternalUnitWithoutChildren(organization.GetRoot())));
                    if (purgeError.HasValue)
                    {
                        _logger.Error("Failed to sync to level 1 prior to disconnecting from FK Org in organization {id}. Error: {code}:{message}", organizationId, purgeError.Value.FailureType, purgeError.Value.Message.GetValueOrDefault());
                        return purgeError;
                    }
                }
                var result = organization.DisconnectOrganizationFromExternalSource(OrganizationUnitOrigin.STS_Organisation);
                if (result.Failed)
                {
                    return result.Error;
                }

                var disconnectionResult = result.Value;
                if (disconnectionResult.RemovedChangeLogs.Any())
                {
                    _stsChangeLogRepository.RemoveRange(disconnectionResult.RemovedChangeLogs);
                }

                foreach (var convertedUnit in disconnectionResult.ConvertedUnits)
                {
                    _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(convertedUnit));
                }
                return Maybe<OperationError>.None;
            });
        }

        private static ExternalOrganizationUnit ToExternalUnitWithoutChildren(OrganizationUnit unit)
        {
            return new ExternalOrganizationUnit(unit.ExternalOriginUuid.GetValueOrDefault(), unit.Name,
                new Dictionary<string, string>(), Array.Empty<ExternalOrganizationUnit>());
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
                _commandBus.Execute<AuthorizedUpdateOrganizationFromFKOrganisationCommand, Maybe<OperationError>>(new AuthorizedUpdateOrganizationFromFKOrganisationCommand(organization, levelsToInclude, subscribeToUpdates, Maybe<ExternalOrganizationUnit>.None))
            );
        }

        public Maybe<OperationError> UnsubscribeFromAutomaticUpdates(Guid organizationId)
        {
            return Modify(organizationId, organization => organization.UnsubscribeFromAutomaticUpdates(OrganizationUnitOrigin.STS_Organisation));
        }

        public Result<IEnumerable<IExternalConnectionChangelog>, OperationError> GetChangeLogs(Guid organizationUuid, int numberOfChangeLogs)
        {
            return GetOrganizationWithImportPermission(organizationUuid)
                .Bind(organization => organization.GetExternalConnectionEntryLogs(OrganizationUnitOrigin.STS_Organisation, numberOfChangeLogs));
        }

        private Result<ExternalOrganizationUnit, OperationError> LoadOrganizationUnits(Organization organization)
        {
            return _stsOrganizationSystemService.ResolveOrganizationTree(organization).Match<Result<ExternalOrganizationUnit, OperationError>>(root => root, detailedOperationError => new OperationError($"Failed to load organization tree:{detailedOperationError.Detail:G}:{detailedOperationError.FailureType:G}:{detailedOperationError.Message}", detailedOperationError.FailureType));
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

        private static Result<ExternalOrganizationUnit, OperationError> ConnectToExternalOrganizationHierarchy(
            Organization organization, ExternalOrganizationUnit importRoot, Maybe<int> levelsToInclude, bool subscribeToUpdates)
        {
            return organization
                .ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, importRoot, levelsToInclude, subscribeToUpdates)
                .Match
                (
                    error => error,
                    () => Result<ExternalOrganizationUnit, OperationError>.Success(importRoot)
                );
        }

        private static OrganizationTreeUpdateConsequences ToConnectionConsequences(ExternalOrganizationUnit importRoot)
        {
            var unitsToImport = importRoot.Flatten();
            var importedTreeToParent = importRoot.ToParentMap(importRoot.ToLookupByUuid());
            var consequences = new OrganizationTreeUpdateConsequences(
                Enumerable.Empty<(Guid, OrganizationUnit)>(),
                Enumerable.Empty<(Guid, OrganizationUnit)>(),
                unitsToImport.Select(unit => (unit, importedTreeToParent[unit.Uuid])).ToList(),
                Enumerable.Empty<(OrganizationUnit affectedUnit, string oldName, string newName)>(),
                Enumerable
                    .Empty<(OrganizationUnit movedUnit, OrganizationUnit oldParent, ExternalOrganizationUnit newParent)>(),
                Maybe<OrganizationRootChange>.None);
            return consequences;
        }
    }
}
