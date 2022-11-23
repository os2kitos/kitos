using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Events;
using Core.DomainModel.Extensions;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Context;
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
        private readonly IDomainEvents _domainEvents;
        private readonly IGenericRepository<OrganizationUnit> _organizationUnitRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly Maybe<ActiveUserIdContext> _activeUserIdContext;
        private readonly IUserRepository _userRepository;
        private readonly IGenericRepository<StsOrganizationChangeLog> _stsChangeLogRepository;

        public StsOrganizationSynchronizationService(
            IAuthorizationContext authorizationContext,
            IStsOrganizationUnitService stsOrganizationUnitService,
            IOrganizationService organizationService,
            ILogger logger,
            IStsOrganizationService stsOrganizationService,
            IDatabaseControl databaseControl,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IGenericRepository<OrganizationUnit> organizationUnitRepository,
            Maybe<ActiveUserIdContext> activeUserIdContext,
            IUserRepository userRepository,
            IGenericRepository<StsOrganizationChangeLog> stsChangeLogRepository)
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
            _activeUserIdContext = activeUserIdContext;
            _userRepository = userRepository;
            _stsChangeLogRepository = stsChangeLogRepository;
        }

        public Result<StsOrganizationSynchronizationDetails, OperationError> GetSynchronizationDetails(Guid organizationId)
        {
            return GetOrganizationWithImportPermission(organizationId)
                .Select(organization =>
                {
                    var currentConnectionStatus = ValidateConnection(organization);
                    var isConnected = organization.StsOrganizationConnection?.Connected == true;
                    var subscribesToUpdates = organization.StsOrganizationConnection?.SubscribeToUpdates == true;
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
                        subscribesToUpdates
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
                    .Bind
                        (
                            importRoot =>
                            {
                                var unitsToImport = importRoot.Flatten();
                                var importedTreeToParent = importRoot.ToParentMap(importRoot.ToLookupByUuid());
                                var consequences = new OrganizationTreeUpdateConsequences(
                                     Enumerable.Empty<(Guid, OrganizationUnit)>(),
                                     Enumerable.Empty<(Guid, OrganizationUnit)>(),
                                    unitsToImport.Select(unit => (unit, importedTreeToParent[unit.Uuid])).ToList(),
                                     Enumerable.Empty<(OrganizationUnit affectedUnit, string oldName, string newName)>(),
                                    Enumerable.Empty<(OrganizationUnit movedUnit, OrganizationUnit oldParent, ExternalOrganizationUnit newParent)>());

                                return WithLogEntries(consequences)
                                    .Bind(logEntries => organization.AddExternalImportLog(OrganizationUnitOrigin.STS_Organisation, logEntries));
                            }
                        )
                    .MatchFailure();
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
                _stsChangeLogRepository.RemoveRange(disconnectionResult.RemovedChangeLogs);

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
                            _organizationUnitRepository.RemoveRange(consequences.DeletedExternalUnitsBeingDeleted.Select(x => x.organizationUnit).ToList());
                        }
                        foreach (var (affectedUnit, _, _) in consequences.OrganizationUnitsBeingRenamed)
                        {
                            _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(affectedUnit));
                        }
                        return consequences;
                    })
                    .Bind(WithLogEntries)
                    .Bind(logEntries =>
                    {
                        return organization.AddExternalImportLog(OrganizationUnitOrigin.STS_Organisation, logEntries)
                            .Match<Result<StsOrganizationChangeLog, OperationError>>
                            (
                                importLogResult =>
                                {
                                    foreach (var removedLog in importLogResult.RemovedChangeLogs)
                                    {
                                        //if a log is part of the AddedChangeLogs, it wasn't yet not saved to the DB
                                        if (importLogResult.AddedChangeLogs.Contains(removedLog))
                                        {
                                            continue;
                                        }
                                        
                                        _stsChangeLogRepository.Delete(removedLog);
                                    }
                                    _stsChangeLogRepository.Save();

                                    return logEntries;
                                },
                                error => error
                            );
                    })
                    .Match(_ => Maybe<OperationError>.None, error => error)
            );
        }

        public Result<IEnumerable<StsOrganizationChangeLog>, OperationError> GetChangeLogs(Guid organizationUuid, int numberOfChangeLogs)
        {
            return GetOrganizationWithImportPermission(organizationUuid)
                .Bind(organization => organization.GetStsOrganizationConnectionEntryLogs(OrganizationUnitOrigin.STS_Organisation, numberOfChangeLogs));
        }

        public IEnumerable<StsOrganizationConsequenceLog> ConvertConsequencesToConsequenceLogs(OrganizationTreeUpdateConsequences consequences)
        {
            var logs = new List<StsOrganizationConsequenceLog>();
            logs.AddRange(MapAddedOrganizationUnits(consequences));
            logs.AddRange(MapRenamedOrganizationUnits(consequences));
            logs.AddRange(MapMovedOrganizationUnits(consequences));
            logs.AddRange(MapRemovedOrganizationUnits(consequences));
            logs.AddRange(MapConvertedOrganizationUnits(consequences));

            return logs;
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

        private Result<ExternalOrganizationUnit, OperationError> ConnectToExternalOrganizationHierarchy(
            Organization organization, ExternalOrganizationUnit importRoot, Maybe<int> levelsToInclude, bool subscribeToUpdates)
        {
            return organization.ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, importRoot, levelsToInclude, subscribeToUpdates)
                .Match
                (
                    error => error,
                    () => Result<ExternalOrganizationUnit, OperationError>.Success(importRoot));
        }

        private Result<StsOrganizationChangeLog, OperationError> WithLogEntries(OrganizationTreeUpdateConsequences consequences)
        {
            var changeLog = new StsOrganizationChangeLog { Origin = StsOrganizationChangeLogOrigin.Background };

            if (_activeUserIdContext.HasValue)
            {
                var userId = _activeUserIdContext.Value.ActiveUserId;
                var user = _userRepository.GetById(userId);
                if (user == null)
                    return new OperationError($"User with id: {userId} was not found", OperationFailure.NotFound);

                changeLog.Origin = StsOrganizationChangeLogOrigin.User;
                changeLog.UserId = userId;
            }

            var logs = ConvertConsequencesToConsequenceLogs(consequences).ToList();
            changeLog.ConsequenceLogs = logs;
            changeLog.LogTime = DateTime.Now;
            
            return changeLog;
        }

        private static IEnumerable<StsOrganizationConsequenceLog> MapConvertedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .DeletedExternalUnitsBeingConvertedToNativeUnits
                .Select(converted => new StsOrganizationConsequenceLog
                {
                    Name = converted.organizationUnit.Name,
                    Type = ConnectionUpdateOrganizationUnitChangeType.Converted,
                    Uuid = converted.externalOriginUuid,
                    Description = $"'{converted.organizationUnit.Name}' er slettet i FK Organisation men konverteres til KITOS enhed, da den anvendes aktivt i KITOS."
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationConsequenceLog> MapRemovedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .DeletedExternalUnitsBeingDeleted
                .Select(deleted => new StsOrganizationConsequenceLog
                {
                    Name = deleted.organizationUnit.Name,
                    Type = ConnectionUpdateOrganizationUnitChangeType.Deleted,
                    Uuid = deleted.externalOriginUuid,
                    Description = $"'{deleted.organizationUnit.Name}' slettes."
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationConsequenceLog> MapMovedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .OrganizationUnitsBeingMoved
                .Select(moved =>
                {
                    var (movedUnit, oldParent, newParent) = moved;
                    return new StsOrganizationConsequenceLog
                    {
                        Name = movedUnit.Name,
                        Type = ConnectionUpdateOrganizationUnitChangeType.Moved,
                        Uuid = movedUnit.ExternalOriginUuid.GetValueOrDefault(),
                        Description = $"'{movedUnit.Name}' flyttes fra at være underenhed til '{oldParent.Name}' til fremover at være underenhed for {newParent.Name}"
                    };
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationConsequenceLog> MapRenamedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .OrganizationUnitsBeingRenamed
                .Select(renamed =>
                {
                    var (affectedUnit, oldName, newName) = renamed;
                    return new StsOrganizationConsequenceLog
                    {
                        Name = oldName,
                        Type = ConnectionUpdateOrganizationUnitChangeType.Renamed,
                        Uuid = affectedUnit.ExternalOriginUuid.GetValueOrDefault(),
                        Description = $"'{oldName}' omdøbes til '{newName}'"
                    };
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationConsequenceLog> MapAddedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .AddedExternalOrganizationUnits
                .Select(added => new StsOrganizationConsequenceLog
                {
                    Name = added.unitToAdd.Name,
                    Type = ConnectionUpdateOrganizationUnitChangeType.Added,
                    Uuid = added.unitToAdd.Uuid,
                    Description = $"'{added.unitToAdd.Name}' tilføjes som underenhed til '{added.parent?.Name}'"
                }
                )
                .ToList();
        }
    }
}
