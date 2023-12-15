using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.Notification;
using Core.ApplicationServices.System;
using Core.ApplicationServices.System.Write;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.ItSystem;
using Core.DomainServices.Repositories.Organization;
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
        private readonly IGlobalAdminNotificationService _globalAdminNotificationService;
        private readonly ITransactionManager _transactionManager;
        private readonly IUserRepository _userRepository;
        private readonly IOperationClock _operationClock;
        private readonly ILogger _logger;
        private readonly IDatabaseControl _databaseControl;
        private readonly IDomainEvents _domainEvents;
        private readonly IItSystemWriteService _writeService;

        public RightsHolderSystemService(
            IOrganizationalUserContext userContext,
            IOrganizationRepository organizationRepository,
            IItSystemService systemService,
            IGlobalAdminNotificationService globalAdminNotificationService,
            ITransactionManager transactionManager,
            IUserRepository userRepository,
            IOperationClock operationClock,
            ILogger logger,
            IDatabaseControl databaseControl,
            IDomainEvents domainEvents,
            IItSystemWriteService writeService)
            : base(userContext, organizationRepository)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
            _systemService = systemService;
            _globalAdminNotificationService = globalAdminNotificationService;
            _transactionManager = transactionManager;
            _userRepository = userRepository;
            _operationClock = operationClock;
            _logger = logger;
            _databaseControl = databaseControl;
            _domainEvents = domainEvents;
            _writeService = writeService;
        }

        public Result<ItSystem, OperationError> CreateNewSystemAsRightsHolder(Guid rightsHolderUuid, RightsHolderSystemCreationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            using var transaction = _transactionManager.Begin();
            try
            {
                var organizationId = _organizationRepository.GetByUuid(rightsHolderUuid).Select(x => x.Id);

                if (organizationId.IsNone)
                    return new OperationError("Invalid rights holder id provided", OperationFailure.BadInput);

                if (!_userContext.HasRole(organizationId.Value, OrganizationRole.RightsHolderAccess))
                    return new OperationError("User does not have rights holder access in the provided organization", OperationFailure.Forbidden);

                var name = parameters.Name;

                if (name.IsUnchanged)
                    return new OperationError("Error must be defined upon creation", OperationFailure.BadInput);

                parameters.Name = OptionalValueChange<string>.None; //name is extracted - make sure it's not re-written pointlessly

                var result = _systemService
                    .CreateNewSystem(organizationId.Value, name.NewValue, parameters.RightsHolderProvidedUuid)
                    .Bind(system => _systemService.UpdateRightsHolder(system.Id, rightsHolderUuid))
                    .Bind(system => ApplyUpdates(system, parameters));

                if (result.Ok)
                {
                    SaveAndNotify(result.Value, transaction);
                }
                else
                {
                    transaction.Rollback();
                    _logger.Error("RightsHolder {uuid} failed to create It-System {name} due to error: {errorMessage}", rightsHolderUuid, parameters.Name, result.Error.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed creating rightsholder system for rightsholder with id {rightsHolderUuid}", rightsHolderUuid);
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        public Result<ItSystem, OperationError> UpdateAsRightsHolder(Guid systemUuid, RightsHolderSystemUpdateParameters parameters)
        {
            using var transaction = _transactionManager.Begin();
            try
            {
                var result = _systemService
                    .GetSystem(systemUuid)
                    .Bind(WithRightsHolderAccessTo)
                    .Bind(WithActiveEntityOnly)
                    .Bind(system => ApplyUpdates(system, parameters));

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

        public Result<ItSystem, OperationError> DeactivateAsRightsHolder(Guid systemUuid, string reason)
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
                    .Bind(Deactivate);

                if (result.Ok)
                {
                    _logger.Information("User {userId} deactivated system with id {systemUuid} due to reason:{reason}", _userContext.UserId, systemUuid, reason);
                    SaveAndNotify(result.Value, transaction);

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
                    transaction.Rollback();
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

        private Result<ItSystem, OperationError> Deactivate(ItSystem system)
        {
            return _writeService.Update(system.Uuid, new SystemUpdateParameters() { Deactivated = true.AsChangedValue() });
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
        private void SaveAndNotify(ItSystem system, IDatabaseTransaction transaction)
        {
            _domainEvents.Raise(new EntityUpdatedEvent<ItSystem>(system));
            _databaseControl.SaveChanges();
            transaction.Commit();
        }

        private Result<ItSystem, OperationError> ApplyUpdates(ItSystem system, SharedSystemUpdateParameters parameters)
        {
            var updateParameters = new SystemUpdateParameters
            {
                Name = parameters.Name,
                ExternalUuid = parameters.ExternalUuid,
                ExternalReferences = parameters.ExternalReferences,
                ParentSystemUuid = parameters.ParentSystemUuid,
                FormerName = parameters.FormerName,
                Description = parameters.Description,
                BusinessTypeUuid = parameters.BusinessTypeUuid,
                TaskRefUuids = parameters.TaskRefUuids
            };

            //Validate that rightsholder is also rights holder of the parent system
            if (updateParameters.ParentSystemUuid.HasChange)
            {
                var newParent = updateParameters.ParentSystemUuid.NewValue;
                if (newParent.HasValue)
                {
                    var parentSystemResult = _systemService
                        .GetSystem(newParent.Value)
                        .Bind(WithRightsHolderAccessTo);
                    if (parentSystemResult.Failed)
                        return parentSystemResult.Error.FailureType == OperationFailure.NotFound
                            ? new OperationError("Parent system cannot be found", OperationFailure.BadInput)
                            : parentSystemResult.Error;
                }
            }

            return _writeService.Update(system.Uuid, updateParameters);
        }
    }
}
