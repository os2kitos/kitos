using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.KLE;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.System;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Options;
using Core.DomainServices.Role;
using Core.DomainServices.SystemUsage;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Serilog;

namespace Core.ApplicationServices.SystemUsage.Write
{
    public class ItSystemUsageWriteService : IItSystemUsageWriteService
    {
        private readonly IItSystemUsageService _systemUsageService;
        private readonly ITransactionManager _transactionManager;
        private readonly IItSystemService _systemService;
        private readonly IOrganizationService _organizationService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOptionsService<ItSystemUsage, ItSystemCategories> _systemCategoriesOptionsService;
        private readonly IItContractService _contractService;
        private readonly IItProjectService _projectService;
        private readonly IKLEApplicationService _kleApplicationService;
        private readonly IDatabaseControl _databaseControl;
        private readonly IDomainEvents _domainEvents;
        private readonly ILogger _logger;
        private readonly IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage> _roleAssignmentService;

        public ItSystemUsageWriteService(
            IItSystemUsageService systemUsageService,
            ITransactionManager transactionManager,
            IItSystemService systemService,
            IOrganizationService organizationService,
            IAuthorizationContext authorizationContext,
            IOptionsService<ItSystemUsage, ItSystemCategories> systemCategoriesOptionsService,
            IItContractService contractService,
            IItProjectService projectService,
            IKLEApplicationService kleApplicationService,
            IDatabaseControl databaseControl,
            IDomainEvents domainEvents,
            ILogger logger,
            IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage> roleAssignmentService)
        {
            _systemUsageService = systemUsageService;
            _transactionManager = transactionManager;
            _systemService = systemService;
            _organizationService = organizationService;
            _authorizationContext = authorizationContext;
            _systemCategoriesOptionsService = systemCategoriesOptionsService;
            _contractService = contractService;
            _projectService = projectService;
            _kleApplicationService = kleApplicationService;
            _databaseControl = databaseControl;
            _domainEvents = domainEvents;
            _logger = logger;
            _roleAssignmentService = roleAssignmentService;
        }

        public Result<ItSystemUsage, OperationError> Create(SystemUsageCreationParameters parameters)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            var systemResult = _systemService.GetSystem(parameters.SystemUuid);
            if (systemResult.Failed)
            {
                _logger.Error("Failed to retrieve itSystem with id {uuid}. Error {error}", parameters.SystemUuid, systemResult.Error.ToString());
                return new OperationError("Unable to resolve IT-System:" + systemResult.Error.Message.GetValueOrFallback(string.Empty), systemResult.Error.FailureType);
            }

            var organizationResult = _organizationService.GetOrganization(parameters.OrganizationUuid);
            if (organizationResult.Failed)
            {
                _logger.Error("Failed to retrieve organization with id {uuid}. Error {error}", parameters.OrganizationUuid, organizationResult.Error.ToString());
                return new OperationError("Unable to resolve IT-System:" + organizationResult.Error.Message.GetValueOrFallback(string.Empty), organizationResult.Error.FailureType);
            }

            var creationResult = _systemUsageService
                .CreateNew(systemResult.Value.Id, organizationResult.Value.Id)
                .Bind(createdSystemUsage => Update(() => createdSystemUsage, parameters.AdditionalValues));

            if (creationResult.Ok)
            {
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return creationResult;
        }

        public Result<ItSystemUsage, OperationError> Update(Guid systemUsageUuid, SystemUsageUpdateParameters parameters)
        {
            return Update(() => _systemUsageService.GetByUuid(systemUsageUuid), parameters);
        }
        private Result<ItSystemUsage, OperationError> Update(Func<Result<ItSystemUsage, OperationError>> getItSystemUsage, SystemUsageUpdateParameters parameters)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

            var result = getItSystemUsage()
                    .Bind(WithWriteAccess)
                    .Bind(systemUsage => PerformUpdates(systemUsage, parameters));

            if (result.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(result.Value));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return result;
        }

        private Result<ItSystemUsage, OperationError> PerformUpdates(ItSystemUsage systemUsage, SystemUsageUpdateParameters parameters)
        {
            //Optionally apply changes across the entire update specification
            return WithOptionalUpdate(systemUsage, parameters.GeneralProperties, PerformGeneralDataPropertiesUpdate)
                    .Bind(usage => WithOptionalUpdate(usage, parameters.Roles, PerformRoleAssignmentUpdates))
                    .Bind(usage => WithOptionalUpdate(usage, parameters.OrganizationalUsage, PerformOrganizationalUsageUpdate))
                    .Bind(usage => WithOptionalUpdate(usage, parameters.KLE, PerformKLEUpdate));
        }

        private Result<ItSystemUsage, OperationError> PerformKLEUpdate(ItSystemUsage systemUsage, UpdatedSystemUsageKLEDeviationParameters changes)
        {
            if (changes.AddedKLEUuids.HasValue || changes.RemovedKLEUuids.HasValue)
            {
                var addedTaskRefs = MapOptionalChangeWithFallback(changes.AddedKLEUuids, systemUsage.TaskRefs.Select(x => x.Uuid).ToList()).GetValueOrFallback(Enumerable.Empty<Guid>());
                var removedTaskRefs = MapOptionalChangeWithFallback(changes.RemovedKLEUuids, systemUsage.TaskRefsOptOut.Select(x => x.Uuid).ToList()).GetValueOrFallback(Enumerable.Empty<Guid>());

                var additions = new List<TaskRef>();
                foreach (var uuid in addedTaskRefs)
                {
                    var result = _kleApplicationService.GetKle(uuid);
                    if (result.Failed)
                    {
                        return new OperationError($"Failed to load KLE with uuid:{uuid}:{result.Error.Message.GetValueOrFallback(string.Empty)}", result.Error.FailureType);
                    }
                    additions.Add(result.Value.kle);
                }

                var removals = new List<TaskRef>();
                foreach (var uuid in removedTaskRefs)
                {
                    var result = _kleApplicationService.GetKle(uuid);
                    if (result.Failed)
                    {
                        return new OperationError($"Failed to load KLE with uuid:{uuid}:{result.Error.Message.GetValueOrFallback(string.Empty)}", result.Error.FailureType);
                    }
                    removals.Add(result.Value.kle);
                }

                return systemUsage.UpdateKLEDeviations(additions, removals).Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage);
            }

            return systemUsage;
        }

        private Result<ItSystemUsage, OperationError> PerformOrganizationalUsageUpdate(ItSystemUsage systemUsage, UpdatedSystemUsageOrganizationalUseParameters updatedParameters)
        {
            if (updatedParameters.ResponsibleOrganizationUnitUuid.HasValue ||
                updatedParameters.UsingOrganizationUnitUuids.HasValue)
            {
                var nextResponsibleOrgUuid = MapOptionalChangeWithFallback(updatedParameters.ResponsibleOrganizationUnitUuid, systemUsage.ResponsibleUsage.FromNullable().Select(x => x.OrganizationUnit.Uuid));
                var nextResponsibleOrg = Maybe<OrganizationUnit>.None;
                if (nextResponsibleOrgUuid.HasValue)
                {
                    var organizationUnitResult = _organizationService.GetOrganizationUnit(nextResponsibleOrgUuid.Value);
                    if (organizationUnitResult.Failed)
                        return new OperationError($"Failed to fetch responsible org unit: {organizationUnitResult.Error.Message.GetValueOrFallback(string.Empty)}", organizationUnitResult.Error.FailureType);
                    nextResponsibleOrg = organizationUnitResult.Value;
                }
                var usingOrganizationUnits = MapOptionalChangeWithFallback(updatedParameters.UsingOrganizationUnitUuids, systemUsage.UsedBy.Select(x => x.OrganizationUnit.Uuid).ToList().FromNullable<IEnumerable<Guid>>());
                var nextUsingOrganizationUnits = new List<OrganizationUnit>();
                if (usingOrganizationUnits.HasValue)
                {
                    var usingOrgUnitUuids = usingOrganizationUnits.Value.ToList();
                    if (usingOrgUnitUuids.Any())
                    {
                        foreach (var usingOrgUnitUuid in usingOrgUnitUuids)
                        {
                            var result = _organizationService.GetOrganizationUnit(usingOrgUnitUuid);
                            if (result.Failed)
                                return new OperationError($"Failed to using org unit with id {usingOrgUnitUuid}: {result.Error.Message.GetValueOrFallback(string.Empty)}", result.Error.FailureType);
                            nextUsingOrganizationUnits.Add(result.Value);
                        }
                    }
                }

                return systemUsage
                    .UpdateOrganizationalUsage(nextUsingOrganizationUnits, nextResponsibleOrg)
                    .Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage);
            }
            //No changes provided - skip
            return systemUsage;
        }

        private Result<ItSystemUsage, OperationError> PerformGeneralDataPropertiesUpdate(ItSystemUsage itSystemUsage, UpdatedSystemUsageGeneralProperties generalProperties)
        {
            return WithOptionalUpdate(itSystemUsage, generalProperties.LocalCallName, (systemUsage, localCallName) => systemUsage.UpdateLocalCallName(localCallName))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.LocalSystemId, (systemUsage, localSystemId) => systemUsage.UpdateLocalSystemId(localSystemId)))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.DataClassificationUuid, UpdateDataClassification))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.Notes, (systemUsage, notes) => systemUsage.Note = notes))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.SystemVersion, (systemUsage, version) => systemUsage.UpdateSystemVersion(version)))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.NumberOfExpectedUsersInterval, UpdateExpectedUsersInterval))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.EnforceActive, (systemUsage, enforceActive) => systemUsage.Active = enforceActive.GetValueOrFallback(false)))
                .Bind(usage => UpdateValidityPeriod(usage, generalProperties))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.MainContractUuid, UpdateMainContract))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.AssociatedProjectUuids, UpdateProjectAssociations));
        }

        private static Result<ItSystemUsage, OperationError> UpdateValidityPeriod(ItSystemUsage usage, UpdatedSystemUsageGeneralProperties generalProperties)
        {
            if (generalProperties.ValidFrom.IsNone && generalProperties.ValidTo.IsNone)
                return usage; //Not changes provided

            var newValidFrom = MapDataTimeOptionalChangeWithFallback(generalProperties.ValidFrom, usage.Concluded);
            var newValidTo = MapDataTimeOptionalChangeWithFallback(generalProperties.ValidTo, usage.ExpirationDate);

            return usage.UpdateSystemValidityPeriod(newValidFrom, newValidTo).Match<Result<ItSystemUsage, OperationError>>(error => error, () => usage);
        }

        private static DateTime? MapDataTimeOptionalChangeWithFallback(Maybe<ChangedValue<Maybe<DateTime>>> optionalChange, DateTime? fallback)
        {
            return optionalChange
                .Select(x => x.Value)
                .Match(changeTo =>
                        changeTo.Match
                        (
                            newValue => newValue, //Client set new value
                            () => (DateTime?)null), //Changed to null by client
                    () => fallback // No change provided - use the fallback
                );
        }

        private static T MapOptionalChangeWithFallback<T>(Maybe<ChangedValue<T>> optionalChange, T fallback)
        {
            return optionalChange
                .Select(x => x.Value)
                .Match(changeTo =>
                        changeTo, //Changed to null by client
                    () => fallback // No change provided - use the fallback
                );
        }

        private Result<ItSystemUsage, OperationError> UpdateProjectAssociations(ItSystemUsage systemUsage, Maybe<IEnumerable<Guid>> projectUuids)
        {
            if (projectUuids.IsNone)
            {
                systemUsage.ResetProjectAssociations();
                return systemUsage;
            }

            var itProjects = new List<ItProject>();
            foreach (var uuid in projectUuids.Value)
            {
                var result = _projectService.GetProject(uuid);

                if (result.Failed)
                    return new OperationError($"Error loading project with id: {uuid}. Error:{result.Error.Message.GetValueOrFallback(string.Empty)}", result.Error.FailureType);

                itProjects.Add(result.Value);
            }

            return systemUsage.SetProjectAssociations(itProjects).Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage);
        }

        private Result<ItSystemUsage, OperationError> UpdateMainContract(ItSystemUsage systemUsage, Maybe<Guid> contractId)
        {
            if (contractId.IsNone)
            {
                systemUsage.ResetMainContract();
                return systemUsage;
            }

            var contractResult = _contractService.GetContract(contractId.Value);
            if (contractResult.Failed)
                return new OperationError($"Failure getting the contract:{contractResult.Error.Message.GetValueOrFallback(string.Empty)}", contractResult.Error.FailureType);

            return systemUsage.SetMainContract(contractResult.Value).Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage);
        }

        private Result<ItSystemUsage, OperationError> UpdateExpectedUsersInterval(ItSystemUsage systemUsage, Maybe<(int lower, int? upperBound)> newInterval)
        {
            if (newInterval.IsNone)
                systemUsage.ResetUserCount();

            return systemUsage.SetExpectedUsersInterval(newInterval.Value).Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage);
        }

        private Maybe<OperationError> UpdateDataClassification(ItSystemUsage systemUsage, Maybe<Guid> dataClassificationOptionId)
        {
            if (dataClassificationOptionId.IsNone)
            {
                systemUsage.ResetSystemCategories();
                return Maybe<OperationError>.None;
            }

            var optionByUuid = _systemCategoriesOptionsService.GetOptionByUuid(systemUsage.OrganizationId, dataClassificationOptionId.Value);

            if (optionByUuid.IsNone)
                return new OperationError("Invalid option id", OperationFailure.BadInput);

            if (!optionByUuid.Value.available)
                return new OperationError("Option is not available in the organization.", OperationFailure.BadInput);

            return systemUsage.UpdateSystemCategories(optionByUuid.Value.option);
        }

        private Result<ItSystemUsage, OperationError> PerformRoleAssignmentUpdates(ItSystemUsage itSystemUsage, UpdatedSystemUsageRoles usageRoles)
        {
            return WithOptionalUpdate(itSystemUsage, usageRoles.UserRolePairs, UpdateRoles);
        }

        private Result<ItSystemUsage, OperationError> UpdateRoles(ItSystemUsage systemUsage, Maybe<IEnumerable<UserRolePair>> userRolePairs)
        {
            if (userRolePairs.IsNone)
            {
                // Remove all
                foreach (var systemUsageRight in systemUsage.Rights.ToList())
                {
                    var removeResult = _roleAssignmentService.RemoveRole(systemUsage, systemUsageRight.Role.Uuid, systemUsageRight.User.Uuid);
                    if (removeResult.Failed)
                        return removeResult.Error;
                }

                return systemUsage;
            }
            

            // Compare lists to find which needs to be remove and which need to be added
            var rightsKeys = systemUsage.Rights.Select(x => new UserRolePair { RoleUuid = x.Role.Uuid, UserUuid = x.User.Uuid }).ToList();
            var userRoleKeys = userRolePairs.Value.ToList();

            var toRemove = rightsKeys.Except(userRoleKeys);
            var toAdd = userRoleKeys.Except(rightsKeys);

            foreach (var userRolePair in toRemove)
            {
                var removeResult = _roleAssignmentService.RemoveRole(systemUsage, userRolePair.RoleUuid, userRolePair.UserUuid);

                if (removeResult.Failed)
                    return removeResult.Error;
            }

            foreach (var userRolePair in toAdd)
            {
                var assignmentResult = _roleAssignmentService.AssignRole(systemUsage, userRolePair.RoleUuid, userRolePair.UserUuid);

                if (assignmentResult.Failed)
                    return assignmentResult.Error;
            }

            return systemUsage;
        }

        private static Result<ItSystemUsage, OperationError> WithOptionalUpdate<TValue>(
            ItSystemUsage systemUsage,
            Maybe<ChangedValue<TValue>> optionalUpdate,
            Func<ItSystemUsage, TValue, Result<ItSystemUsage, OperationError>> updateCommand)
        {
            return optionalUpdate
                .Select(changedValue => updateCommand(systemUsage, changedValue.Value))
                .Match(updateResult => updateResult, () => systemUsage);
        }

        private static Result<ItSystemUsage, OperationError> WithOptionalUpdate<TValue>(
            ItSystemUsage systemUsage,
            Maybe<TValue> optionalUpdate,
            Func<ItSystemUsage, TValue, Result<ItSystemUsage, OperationError>> updateCommand)
        {
            return optionalUpdate
                .Select(changedValue => updateCommand(systemUsage, changedValue))
                .Match(updateResult => updateResult, () => systemUsage);
        }

        private static Result<ItSystemUsage, OperationError> WithOptionalUpdate<TValue>(
            ItSystemUsage systemUsage,
            Maybe<ChangedValue<TValue>> optionalUpdate,
            Func<ItSystemUsage, TValue, Maybe<OperationError>> updateCommand)
        {
            return optionalUpdate
                .Select(changedValue => updateCommand(systemUsage, changedValue.Value).Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage))
                .Match(updateResult => updateResult, () => systemUsage);
        }

        private static Result<ItSystemUsage, OperationError> WithOptionalUpdate<TValue>(
            ItSystemUsage systemUsage,
            Maybe<ChangedValue<TValue>> optionalUpdate,
            Action<ItSystemUsage, TValue> updateCommand)
        {
            return optionalUpdate
                .Select(changedValue =>
                {
                    updateCommand(systemUsage, changedValue.Value);
                    return systemUsage;
                })
                .Match(updateResult => updateResult, () => systemUsage);
        }

        private Result<ItSystemUsage, OperationError> WithWriteAccess(ItSystemUsage systemUsage)
        {
            return _authorizationContext.AllowModify(systemUsage) ? systemUsage : new OperationError(OperationFailure.Forbidden);
        }

        public Maybe<OperationError> Delete(Guid itSystemUsageUuid)
        {
            return _systemService.GetSystem(itSystemUsageUuid)
                .Select(systemUsage => _systemService.Delete(systemUsage.Id))
                .Match(systemDeleteResult =>
                {
                    return systemDeleteResult switch
                    {
                        SystemDeleteResult.Ok => Maybe<OperationError>.None,
                        SystemDeleteResult.Forbidden => new OperationError(OperationFailure.Forbidden),
                        SystemDeleteResult.NotFound => new OperationError(OperationFailure.NotFound),
                        _ => new OperationError($"Failed to delete system usage:{systemDeleteResult:G}",
                            OperationFailure.UnknownError)
                    };
                }, error => error);
        }
    }
}
