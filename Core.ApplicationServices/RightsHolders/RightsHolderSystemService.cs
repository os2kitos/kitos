using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.Notification;
using Core.ApplicationServices.References;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainServices;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.ItSystem;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.TaskRefs;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.RightsHolders
{
    public class RightsHolderSystemService : BaseRightsHolderService, IRightsHolderSystemService
    {
        private readonly IOrganizationalUserContext _userContext;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IItSystemService _systemService;
        private readonly ITaskRefRepository _taskRefRepository;
        private readonly IGlobalAdminNotificationService _globalAdminNotificationService;
        private readonly ITransactionManager _transactionManager;
        private readonly IUserRepository _userRepository;
        private readonly IOperationClock _operationClock;
        private readonly ILogger _logger;
        private readonly IReferenceService _referenceService;

        public RightsHolderSystemService(
            IOrganizationalUserContext userContext,
            IOrganizationRepository organizationRepository,
            IItSystemService systemService,
            ITaskRefRepository taskRefRepository,
            IGlobalAdminNotificationService globalAdminNotificationService,
            ITransactionManager transactionManager,
            IUserRepository userRepository,
            IOperationClock operationClock,
            ILogger logger, 
            IReferenceService referenceService)
            : base(userContext, organizationRepository)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
            _systemService = systemService;
            _taskRefRepository = taskRefRepository;
            _globalAdminNotificationService = globalAdminNotificationService;
            _transactionManager = transactionManager;
            _userRepository = userRepository;
            _operationClock = operationClock;
            _logger = logger;
            _referenceService = referenceService;
        }

        public Result<ItSystem, OperationError> CreateNewSystem(Guid rightsHolderUuid, RightsHolderSystemCreationParameters creationParameters)
        {
            if (creationParameters == null)
                throw new ArgumentNullException(nameof(creationParameters));

            using var transaction = _transactionManager.Begin();
            try
            {
                var organizationId = _organizationRepository.GetByUuid(rightsHolderUuid).Select(x => x.Id);

                if (organizationId.IsNone)
                    return new OperationError("Invalid rights holder id provided", OperationFailure.BadInput);

                if (!_userContext.HasRole(organizationId.Value, OrganizationRole.RightsHolderAccess))
                    return new OperationError("User does not have rights holder access in the provided organization", OperationFailure.Forbidden);

                var name = creationParameters.Name;

                if (name.IsUnchanged)
                    return new OperationError("Error must be defined upon creation", OperationFailure.BadInput);

                creationParameters.Name = OptionalValueChange<string>.None; //name is extracted - make sure it's not re-written pointlessly

                var result = _systemService
                    .CreateNewSystem(organizationId.Value, name.NewValue, creationParameters.RightsHolderProvidedUuid)
                    .Bind(system => _systemService.UpdateRightsHolder(system.Id, rightsHolderUuid))
                    .Bind(system => ApplyUpdates(system, creationParameters));

                if (result.Ok)
                {
                    transaction.Commit();
                }
                else
                {
                    _logger.Error("RightsHolder {uuid} failed to create It-System {name} due to error: {errorMessage}", rightsHolderUuid, creationParameters.Name, result.Error.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed creating rightsholder system for rightsholder with id {rightsHolderUuid}", rightsHolderUuid);
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        public Result<ItSystem, OperationError> Update(Guid systemUuid, RightsHolderSystemUpdateParameters updateParameters)
        {
            using var transaction = _transactionManager.Begin();
            try
            {
                var result = _systemService
                    .GetSystem(systemUuid)
                    .Bind(WithRightsHolderAccessTo)
                    .Bind(WithActiveEntityOnly)
                    .Bind(system => ApplyUpdates(system, updateParameters));

                if (result.Ok)
                {
                    transaction.Commit();
                }
                else
                {
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

        public Result<ItSystem, OperationError> Deactivate(Guid systemUuid, string reason)
        {
            if (string.IsNullOrEmpty(reason))
                return new OperationError("No deactivation reason provided", OperationFailure.BadInput);

            using var transaction = _transactionManager.Begin();
            try
            {
                var result = _systemService
                    .GetSystem(systemUuid)
                    .Bind(WithRightsHolderAccessTo)
                    .Bind(WithActiveEntityOnly)
                    .Bind(system => _systemService.Deactivate(system.Id));

                if (result.Ok)
                {
                    _logger.Information("User {userId} deactivated system with id {systemUuid} due to reason:{reason}", _userContext.UserId, systemUuid, reason);
                    transaction.Commit();

                    var currentUserEmail = _userRepository.GetById(_userContext.UserId).Email;
                    var deactivatedItSystem = result.Value;
                    const string subject = "IT-System blev deaktiveret af rettighedshaver";
                    var content =
                        $"<p>IT-Systemet <b>'{deactivatedItSystem.Name}'</b> blev deaktiveret af rettighedshaver.</p>" +
                        "<p>Detaljer:</p>" +
                        "<ul>" +
                        $"<li>Navn: {deactivatedItSystem.Name}</li>" +
                        $"<li>UUID: {deactivatedItSystem.Uuid}</li>" +
                        $"<li>Årsag til deaktivering: {reason}</li>" +
                        $"<li>Rettighedshaver: {deactivatedItSystem.BelongsTo?.Name}</li>" +
                        $"<li>Ansvarlig for deaktivering (email): {currentUserEmail}</li>" +
                        "</ul>";

                    _globalAdminNotificationService.Submit(new GlobalAdminNotification(_operationClock.Now, _userContext.UserId, subject, new GlobalAdminNotificationMessage(content, true)));
                }
                else
                {
                    _logger.Error("User {id} failed to deactivate It-System {uuid} due to error: {errorMessage}", _userContext.UserId, systemUuid, result.Error.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "User {id} Failed deactivating system with uuid {uuid}", _userContext.UserId, systemUuid);
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        private Result<ItSystem, OperationError> ApplyUpdates(ItSystem system, RightsHolderSystemUpdateParameters updates)
        {
            return system
                .WithOptionalUpdate(updates.Name, (itSystem, newValue) => _systemService.UpdateName(itSystem.Id, newValue))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.FormerName, (itSystem, newValue) => _systemService.UpdatePreviousName(itSystem.Id, newValue)))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.Description, (itSystem, newValue) => _systemService.UpdateDescription(itSystem.Id, newValue)))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.ExternalReferences, UpdateExternalReferences))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.ParentSystemUuid, UpdateParentSystem))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.BusinessTypeUuid, (itSystem, newValue) => _systemService.UpdateBusinessType(itSystem.Id, newValue)))
                .Bind(updatedSystem => updatedSystem.WithOptionalUpdate(updates.TaskRefUuids, UpdateTaskRefs));
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
                //Make sure that user has rightsholders access to the parent system
                var parentSystemResult =
                    _systemService
                        .GetSystem(parentSystemUuid.Value)
                        .Bind(WithRightsHolderAccessTo);

                if (parentSystemResult.Failed)
                    return parentSystemResult.Error.FailureType == OperationFailure.NotFound
                        ? new OperationError("Parent system cannot be found", OperationFailure.BadInput)
                        : parentSystemResult.Error;


                parentSystemId = parentSystemResult.Value.Id;
            }

            return _systemService.UpdateParentSystem(system.Id, parentSystemId);

        }

        public Result<IQueryable<ItSystem>, OperationError> GetSystemsWhereAuthenticatedUserHasRightsHolderAccess(IEnumerable<IDomainQuery<ItSystem>> refinements, Guid? rightsHolderUuid = null)
        {
            if (refinements == null)
                throw new ArgumentNullException(nameof(refinements));

            return WithAnyRightsHoldersAccess()
                .Match
                (
                    error => error,
                    () =>
                    {
                        var subQueries = new List<IDomainQuery<ItSystem>>(refinements);
                        var organizationIdsWhereUserHasRightsHoldersAccess = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess);
                        subQueries.Add(new QueryByRightsHolderIdOrOwnOrganizationIds(organizationIdsWhereUserHasRightsHoldersAccess));

                        if (rightsHolderUuid.HasValue)
                        {
                            var organizationId = _organizationRepository.GetByUuid(rightsHolderUuid.Value).Select(x => x.Id);

                            if (organizationId.IsNone)
                                return new OperationError("Invalid organization id", OperationFailure.BadInput);

                            if (!_userContext.HasRole(organizationId.Value, OrganizationRole.RightsHolderAccess))
                            {
                                return new OperationError("User is not rightsholder in the provided organization", OperationFailure.Forbidden);
                            }
                            subQueries.Add(new QueryByRightsHolderUuid(rightsHolderUuid.Value));
                        }

                        var systems = _systemService.GetAvailableSystems(subQueries.ToArray());

                        return Result<IQueryable<ItSystem>, OperationError>.Success(systems);
                    }
                );
        }

        public Result<ItSystem, OperationError> GetSystemAsRightsHolder(Guid systemUuid)
        {
            return WithAnyRightsHoldersAccess()
                .Match
                (
                    error => error,
                    () => _systemService.GetSystem(systemUuid).Bind(WithRightsHolderAccessTo)
                );
        }
    }
}
