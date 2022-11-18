using System;
using System.Collections.Generic;
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
        private readonly IUserService _userService;

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
            IUserService userService)
        {
            _stsOrganizationUnitService = stsOrganizationUnitService;
            _organizationService = organizationService;
            _logger = logger;
            _stsOrganizationService = stsOrganizationService;
            _databaseControl = databaseControl;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _organizationUnitRepository = organizationUnitRepository;
            _userService = userService;
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

        public Maybe<OperationError> Connect(Guid organizationId, Maybe<int> levelsToInclude)
        {
            return Modify(organizationId, organization =>
            {
                return LoadOrganizationUnits(organization)
                    .Match
                    (
                        importRoot =>
                        {
                            var error = organization.ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, importRoot, levelsToInclude);
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

        public Maybe<OperationError> UpdateConnection(Guid organizationId, Maybe<int> levelsToInclude, Guid? userUuid = null)
        {
            return Modify(organizationId, organization =>
                LoadOrganizationUnits(organization)
                    .Bind(importRoot => organization.UpdateConnectionToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, importRoot, levelsToInclude))
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
                    .Bind(consequences =>
                    {
                        var error = LogChanges(organization.StsOrganizationConnection, consequences, userUuid);
                        return error.HasValue 
                            ? error.Value 
                            : Result<Result<OrganizationTreeUpdateConsequences, OperationError>, OperationError>.Success(consequences);
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

        private Maybe<OperationError> LogChanges(StsOrganizationConnection connection, OrganizationTreeUpdateConsequences consequences, Guid? userUuid = null)
        {
            var logs = new List<StsOrganizationChangeLog>();
            logs.AddRange(MapAddedOrganizationUnits(consequences));
            logs.AddRange(MapRenamedOrganizationUnits(consequences));
            logs.AddRange(MapMovedOrganizationUnits(consequences));
            logs.AddRange(MapRemovedOrganizationUnits(consequences));
            logs.AddRange(MapConvertedOrganizationUnits(consequences));

            var entityResponsibleForUpdate = "FK Organisation";
            if (userUuid != null)
            {
                var userResult = _userService.GetUserInOrganization(connection.Organization.Uuid, userUuid.GetValueOrDefault());
                if (userResult.Failed)
                    return userResult.Error;
                var user = userResult.Value;

                entityResponsibleForUpdate = $"{user.GetFullName()} {user.Email}";
            }

            foreach (var log in logs)
            {
                log.ResponsibleEntityName = entityResponsibleForUpdate;

                connection.StsOrganizationChangeLogs.Add(log);
            }
            return Maybe<OperationError>.None;
        }

        private static IEnumerable<StsOrganizationChangeLog> MapConvertedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .DeletedExternalUnitsBeingConvertedToNativeUnits
                .Select(converted => new StsOrganizationChangeLog
                {
                    Name = converted.Name,
                    Type = ConnectionUpdateOrganizationUnitChangeType.Converted,
                    Uuid = converted.ExternalOriginUuid.GetValueOrDefault(),
                    Description = $"'{converted.Name}' er slettet i FK Organisation men konverteres til KITOS enhed, da den anvendes aktivt i KITOS."
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationChangeLog> MapRemovedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .DeletedExternalUnitsBeingDeleted
                .Select(deleted => new StsOrganizationChangeLog
                {
                    Name = deleted.Name,
                    Type = ConnectionUpdateOrganizationUnitChangeType.Deleted,
                    Uuid = deleted.ExternalOriginUuid.GetValueOrDefault(),
                    Description = $"'{deleted.Name}' slettes."
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationChangeLog> MapMovedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .OrganizationUnitsBeingMoved
                .Select(moved =>
                {
                    var (movedUnit, oldParent, newParent) = moved;
                    return new StsOrganizationChangeLog
                    {
                        Name = movedUnit.Name,
                        Type = ConnectionUpdateOrganizationUnitChangeType.Moved,
                        Uuid = movedUnit.ExternalOriginUuid.GetValueOrDefault(),
                        Description = $"'{movedUnit.Name}' flyttes fra at være underenhed til '{oldParent.Name}' til fremover at være underenhed for {newParent.Name}"
                    };
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationChangeLog> MapRenamedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .OrganizationUnitsBeingRenamed
                .Select(renamed =>
                {
                    var (affectedUnit, oldName, newName) = renamed;
                    return new StsOrganizationChangeLog
                    {
                        Name = oldName,
                        Type = ConnectionUpdateOrganizationUnitChangeType.Renamed,
                        Uuid = affectedUnit.ExternalOriginUuid.GetValueOrDefault(),
                        Description = $"'{oldName}' omdøbes til '{newName}'"
                    };
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationChangeLog> MapAddedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .AddedExternalOrganizationUnits
                .Select(added => new StsOrganizationChangeLog
                {
                    Name = added.unitToAdd.Name,
                    Type = ConnectionUpdateOrganizationUnitChangeType.Added,
                    Uuid = added.unitToAdd.Uuid,
                    Description = $"'{added.unitToAdd.Name}' tilføjes som underenhed til '{added.parent.Name}'"
                }
                )
                .ToList();
        }
    }
}
