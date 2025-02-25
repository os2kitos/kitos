using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.References;
using Core.DomainModel.Events;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.References;
using Core.DomainServices.Generic;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.TaskRefs;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.System.Write
{
    public class ItSystemWriteService : IItSystemWriteService
    {
        private readonly IOrganizationalUserContext _userContext;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IItSystemService _systemService;
        private readonly ITaskRefRepository _taskRefRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;
        private readonly IReferenceService _referenceService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IDatabaseControl _databaseControl;
        private readonly IDomainEvents _domainEvents;
        private readonly IEntityIdentityResolver _identityResolver;

        public ItSystemWriteService(
            IOrganizationalUserContext userContext,
            IOrganizationRepository organizationRepository,
            IItSystemService systemService,
            ITaskRefRepository taskRefRepository,
            ITransactionManager transactionManager,
            ILogger logger,
            IReferenceService referenceService,
            IAuthorizationContext authorizationContext,
            IDatabaseControl databaseControl,
            IDomainEvents domainEvents,
            IEntityIdentityResolver identityResolver)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
            _systemService = systemService;
            _taskRefRepository = taskRefRepository;
            _transactionManager = transactionManager;
            _logger = logger;
            _referenceService = referenceService;
            _authorizationContext = authorizationContext;
            _databaseControl = databaseControl;
            _domainEvents = domainEvents;
            _identityResolver = identityResolver;
        }

        public Result<ItSystem, OperationError> CreateNewSystem(Guid organizationUuid, SystemUpdateParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            using var transaction = _transactionManager.Begin();
            try
            {
                var organizationId = _organizationRepository.GetByUuid(organizationUuid).Select(x => x.Id);

                if (organizationId.IsNone)
                    return new OperationError("Invalid org uuid provided", OperationFailure.BadInput);

                var name = parameters.Name;

                if (name.IsUnchanged)
                    return new OperationError("Error must be defined upon creation", OperationFailure.BadInput);

                parameters.Name = OptionalValueChange<string>.None; //name is extracted - make sure it's not re-written pointlessly

                var result = _systemService
                    .CreateNewSystem(organizationId.Value, name.NewValue)
                    .Bind(system => ApplyUpdates(system, parameters));

                if (result.Ok)
                {
                    SaveAndNotify(result.Value, transaction);
                }
                else
                {
                    transaction.Rollback();
                    _logger.Error("Failed to create It-System {name} due to error: {errorMessage}", name, result.Error.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed creating system");
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        public Result<ItSystem, OperationError> Update(Guid systemUuid, SystemUpdateParameters parameters)
        {
            return PerformUpdateTransaction(systemUuid, system => ApplyUpdates(system, parameters), WithWriteAccess);
        }

        public Result<ItSystem, OperationError> Delete(Guid systemUuid)
        {
            return _systemService
                .GetSystem(systemUuid)
                .Bind<ItSystem>(system =>
                {
                    var deleteResult = _systemService.Delete(system.Id, false /*Do not break binding. Dependencies must be removed before deleting master data through this service*/);
                    if (deleteResult == SystemDeleteResult.Ok)
                    {
                        return system;
                    }

                    return new OperationError(deleteResult.ToString("G"), OperationFailure.UnknownError);
                });
        }

        public Result<ItSystem, OperationError> DBSUpdate(Guid systemUuid, DBSUpdateParameters parameters)
        {
            return PerformUpdateTransaction(systemUuid, system => ApplyDBSUpdates(system, parameters), WithDBSWriteAccess);
        }

        public Result<ExternalReference, OperationError> AddExternalReference(Guid systemUuid, ExternalReferenceProperties externalReferenceProperties)
        {
            return GetSystemAndAuthorizeAccess(systemUuid)
                .Bind(usage => _referenceService.AddReference(usage.Id, ReferenceRootType.System, externalReferenceProperties));
        }

        public Result<ExternalReference, OperationError> UpdateExternalReference(Guid systemUuid, Guid externalReferenceUuid,
            ExternalReferenceProperties externalReferenceProperties)
        {
            return GetSystemAndAuthorizeAccess(systemUuid)
                .Bind(usage => _referenceService.UpdateReference(usage.Id, ReferenceRootType.System, externalReferenceUuid, externalReferenceProperties));
        }

        public Result<ExternalReference, OperationError> DeleteExternalReference(Guid systemUuid, Guid externalReferenceUuid)
        {
            return GetSystemAndAuthorizeAccess(systemUuid)
                .Bind(_ =>
                {
                    var getIdResult = _identityResolver.ResolveDbId<ExternalReference>(externalReferenceUuid);
                    if (getIdResult.IsNone)
                        return new OperationError($"ExternalReference with uuid: {externalReferenceUuid} was not found", OperationFailure.NotFound);
                    var externalReferenceId = getIdResult.Value;

                    return _referenceService.DeleteByReferenceId(externalReferenceId)
                        .Match(Result<ExternalReference, OperationError>.Success,
                            operationFailure =>
                                new OperationError($"Failed to remove the ExternalReference with uuid: {externalReferenceUuid}", operationFailure));
                });
        }

        private Result<ItSystem, OperationError> PerformUpdateTransaction(Guid systemUuid,
            Func<ItSystem, Result<ItSystem, OperationError>> mutation, Func<ItSystem, Result<ItSystem, OperationError>> authorizeMethod)
        {
            using var transaction = _transactionManager.Begin();
            try
            {
                var result = _systemService.GetSystem(systemUuid)
                    .Bind(authorizeMethod)
                    .Bind(mutation);

                if (result.Ok)
                {
                    SaveAndNotify(result.Value, transaction);
                }
                else
                {
                    transaction.Rollback();
                    _logger.Error("User {id} failed to update It-System {uuid} due to error: {errorMessage}", _userContext.UserId, systemUuid, result.Error.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "User {id} Failed updating system with uuid {uuid}", _userContext.UserId, systemUuid);
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        private Result<ItSystem, OperationError> WithWriteAccess(ItSystem system)
        {
            return _authorizationContext.AllowModify(system) ? system : new OperationError(OperationFailure.Forbidden);
        }

        private Result<ItSystem, OperationError> ApplyUpdates(ItSystem system, SystemUpdateParameters updates)
        {
            return system
                .WithOptionalUpdate(updates.Name, (itSystem, newValue) => _systemService.UpdateName(itSystem.Id, newValue))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.ExternalUuid, (itSystem, newValue) => _systemService.UpdateExternalUuid(itSystem.Id, newValue)))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.FormerName, (itSystem, newValue) => _systemService.UpdatePreviousName(itSystem.Id, newValue)))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.Description, (itSystem, newValue) => _systemService.UpdateDescription(itSystem.Id, newValue)))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.ExternalReferences, UpdateExternalReferences))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.ParentSystemUuid, UpdateParentSystem))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.BusinessTypeUuid, (itSystem, newValue) => _systemService.UpdateBusinessType(itSystem.Id, newValue)))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.TaskRefUuids, UpdateTaskRefs))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.ArchivingRecommendation, UpdateUpdateRecommendedArchiveDuty))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.RightsHolderUuid, (itSystem, newValue) => _systemService.UpdateRightsHolder(itSystem.Id, newValue)))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.Scope, (itSystem, newValue) => _systemService.UpdateAccessModifier(itSystem.Id, newValue)))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.Deactivated, HandleDeactivatedState));
        }

        private Result<ItSystem, OperationError> ApplyDBSUpdates(ItSystem itSystem, DBSUpdateParameters parameters)
        {
            return itSystem.WithOptionalUpdate(parameters.SystemName, (sys, dbsName) => sys.UpdateDBSName(dbsName))
                .Bind(system => system.WithOptionalUpdate(parameters.DataProcessorName, (sys, dbsDataProcessorName) => sys.UpdateDBSDataProcessorName(dbsDataProcessorName)));

        }

        private Result<ItSystem, OperationError> HandleDeactivatedState(ItSystem itSystem, bool deactivated)
        {
            return deactivated ? _systemService.Deactivate(itSystem.Id) : _systemService.Activate(itSystem.Id);
        }

        private static Maybe<OperationError> UpdateUpdateRecommendedArchiveDuty(ItSystem system, (OptionalValueChange<ArchiveDutyRecommendationTypes?> recommendation, OptionalValueChange<string> comment) newValue)
        {
            var newRecommendation = newValue.recommendation.Match(changed => changed, () => system.ArchiveDuty);
            var newComment = newValue.comment.Match(comment => comment, () => system.ArchiveDutyComment);
            return system.UpdateRecommendedArchiveDuty(newRecommendation, newComment);
        }

        private Result<ItSystem, OperationError> UpdateExternalReferences(ItSystem system, IEnumerable<UpdatedExternalReferenceProperties> externalReferences)
        {
            //Clear existing state
            var updateResult = _referenceService.UpdateExternalReferences(
                ReferenceRootType.System,
                system.Id,
                externalReferences);

            if (updateResult.HasValue)
                return new OperationError($"Failed to update references with error message: {updateResult.Value.Message.GetValueOrEmptyString()}", updateResult.Value.FailureType);

            return system;
        }

        private Result<ItSystem, OperationError> UpdateTaskRefs(ItSystem system, IEnumerable<Guid> taskRefUuidsChanges)
        {
            var taskRefUuids = taskRefUuidsChanges.ToList();

            var taskRefIds = new HashSet<int>();
            foreach (var uuid in taskRefUuids)
            {
                var taskRef = _taskRefRepository.GetTaskRef(uuid);

                if (taskRef.IsNone)
                    return new OperationError($"Invalid KLE UUID:{uuid}", OperationFailure.BadInput);

                var taskRefValue = taskRef.Value;

                if (!taskRefIds.Add(taskRefValue.Id))
                    return new OperationError($"Overlapping KLE. Please specify the same KLE only once. KLE resolved by uuid {uuid} which matches overlap on KLE {taskRefValue.TaskKey}", OperationFailure.BadInput);
            }

            return _systemService.UpdateTaskRefs(system.Id, taskRefIds.ToList());
        }

        private Result<ItSystem, OperationError> UpdateParentSystem(ItSystem system, Guid? parentSystemUuid)
        {
            var parentSystemId = default(int?);
            if (parentSystemUuid.HasValue)
            {
                var parentSystemResult = _systemService.GetSystem(parentSystemUuid.Value);

                if (parentSystemResult.Failed)
                    return parentSystemResult.Error.FailureType == OperationFailure.NotFound
                        ? new OperationError("Parent system cannot be found", OperationFailure.BadInput)
                        : parentSystemResult.Error;


                parentSystemId = parentSystemResult.Value.Id;
            }

            return _systemService.UpdateParentSystem(system.Id, parentSystemId);

        }

        private void SaveAndNotify(ItSystem system, IDatabaseTransaction transaction)
        {
            _domainEvents.Raise(new EntityUpdatedEvent<ItSystem>(system));
            _databaseControl.SaveChanges();
            transaction.Commit();
        }

        private Result<ItSystem, OperationError> GetSystemAndAuthorizeAccess(Guid systemUuid)
        {
            return _systemService
                .GetSystem(systemUuid)
                .Bind(WithWriteAccess);
        }

        private Result<ItSystem, OperationError> WithDBSWriteAccess(ItSystem system)
        {
            return WithWriteAccess(system); //Placeholder for now
        }
    }
}
