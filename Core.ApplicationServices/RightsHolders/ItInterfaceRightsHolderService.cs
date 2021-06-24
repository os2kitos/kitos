using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Notification;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Interface;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Core.ApplicationServices.RightsHolders
{
    public class ItInterfaceRightsHolderService : BaseRightsHolderService, IItInterfaceRightsHolderService
    {
        private readonly IOrganizationalUserContext _userContext;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IItSystemService _systemService;
        private readonly IItInterfaceService _itInterfaceService;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger; 
        private readonly IGlobalAdminNotificationService _globalAdminNotificationService;
        private readonly IUserRepository _userRepository;
        private readonly IOperationClock _operationClock;

        public ItInterfaceRightsHolderService(
            IOrganizationalUserContext userContext,
            IOrganizationRepository organizationRepository,
            IItSystemService systemService,
            IItInterfaceService itInterfaceService,
            ITransactionManager transactionManager,
            ILogger logger, 
            IGlobalAdminNotificationService globalAdminNotificationService, 
            IUserRepository userRepository, 
            IOperationClock operationClock) : base(userContext, organizationRepository)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
            _itInterfaceService = itInterfaceService;
            _transactionManager = transactionManager;
            _systemService = systemService;
            _logger = logger;
            _globalAdminNotificationService = globalAdminNotificationService;
            _userRepository = userRepository;
            _operationClock = operationClock;
        }

        public Result<ItInterface, OperationError> CreateNewItInterface(Guid rightsHolderUuid, RightsHolderItInterfaceCreationParameters creationParameters)
        {
            if (creationParameters == null)
                throw new ArgumentNullException(nameof(creationParameters));

            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            try
            {
                var organizationId = _organizationRepository.GetByUuid(rightsHolderUuid).Select(x => x.Id);

                if (organizationId.IsNone)
                    return new OperationError("Invalid rights holder id provided", OperationFailure.BadInput);

                var exposingSystem = _systemService.GetSystem(creationParameters.ExposingSystemUuid);

                if (exposingSystem.Failed)
                {
                    if (exposingSystem.Error.FailureType == OperationFailure.NotFound) //If we can't find the exposing system the call will never work and should return BadInput.
                        return new OperationError("Invalid exposing system id provided", OperationFailure.BadInput);
                    return exposingSystem.Error;
                }

                if (!_userContext.HasRole(organizationId.Value, OrganizationRole.RightsHolderAccess))
                    return new OperationError("User does not have rightsholder access in the provided organization", OperationFailure.Forbidden);

                var result = _itInterfaceService
                    .CreateNewItInterface(organizationId.Value, creationParameters.Name, creationParameters.InterfaceId, creationParameters.RightsHolderProvidedUuid)
                    .Bind(ItInterface => ApplyUpdates(ItInterface, creationParameters, exposingSystem.Value.Id));

                if (result.Ok)
                {
                    transaction.Commit();
                }
                else
                {
                    _logger.Error("RightsHolder {uuid} failed to create It-Interface {name} due to error: {errorMessage}", rightsHolderUuid, creationParameters.Name, result.Error.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed creating rightsholder It-Interface for rightsholder with id {uuid}", rightsHolderUuid);
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        public Result<ItInterface, OperationError> Deactivate(Guid interfaceUuid, string reason)
        {
            if (string.IsNullOrEmpty(reason))
                return new OperationError("No deactivation reason provided", OperationFailure.BadInput);

            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            try
            {
                var result = _itInterfaceService
                    .GetInterface(interfaceUuid)
                    .Bind(WithRightsHolderAccessTo)
                    .Bind(WithActiveEntityOnly)
                    .Bind(itInterface => _itInterfaceService.Deactivate(itInterface.Id));

                if (result.Ok)
                {
                    _logger.Information("User {userId} deactivated It-Interface with uuid: {uuid} due to reason: {reason}", _userContext.UserId, interfaceUuid, reason);
                    transaction.Commit();

                    var currentUserEmail = _userRepository.GetById(_userContext.UserId).Email;
                    var deactivatedItInterface = result.Value;
                    const string subject = "Snitflade blev deaktiveret af rettighedshaver";
                    var content =
                        $"<p>Snitfladen <b>'{deactivatedItInterface.Name}'</b> blev deaktiveret af rettighedshaver.</p>" +
                        "<p>Detaljer:</p>" +
                        "<ul>" +
                        $"<li>Navn: {deactivatedItInterface.Name}</li>" +
                        $"<li>UUID: {deactivatedItInterface.Uuid}</li>" +
                        $"<li>Årsag til deaktivering: {reason}</li>" +
                        $"<li>Rettighedshaver: {deactivatedItInterface.ExhibitedBy?.ItSystem?.BelongsTo?.Name}</li>" +
                        $"<li>Ansvarlig for deaktivering (email): {currentUserEmail}</li>" +
                        "</ul>";

                    _globalAdminNotificationService.Submit(new GlobalAdminNotification(_operationClock.Now, _userContext.UserId, subject, new GlobalAdminNotificationMessage(content, true)));
                }
                else
                {
                    _logger.Error("User {userId} failed to deactivate It-Interface with uuid: {uuid} due to error: {errorMessage}", _userContext.UserId, interfaceUuid, result.Error.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "User {userId} Failed deactivating It-Interface with uuid: {uuid}", _userContext.UserId, interfaceUuid);
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        public Result<ItInterface, OperationError> GetInterfaceAsRightsHolder(Guid interfaceUuid)
        {
            return WithAnyRightsHoldersAccess()
                .Match
                (
                    error => error,
                    () => _itInterfaceService.GetInterface(interfaceUuid).Bind(WithRightsHolderAccessTo)
                );
        }

        public Result<IQueryable<ItInterface>, OperationError> GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess(IEnumerable<IDomainQuery<ItInterface>> refinements, Guid? rightsHolderUuid = null)
        {
            if (refinements == null)
                throw new ArgumentNullException(nameof(refinements));

            return WithAnyRightsHoldersAccess()
                .Match
                (
                    error => error,
                    () =>
                    {
                        var subQueries = new List<IDomainQuery<ItInterface>>();
                        var organizationIdsWhereUserHasRightsHoldersAccess = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess);
                        subQueries.Add(new QueryByRightsHolderIdsOrOwnOrganizationIds(organizationIdsWhereUserHasRightsHoldersAccess));

                        if (rightsHolderUuid.HasValue)
                        {
                            var organizationId = _organizationRepository.GetByUuid(rightsHolderUuid.Value).Select(x => x.Id);

                            if (organizationId.IsNone)
                                return new OperationError("Invalid organization id", OperationFailure.BadInput);

                            if (!_userContext.HasRole(organizationId.Value, OrganizationRole.RightsHolderAccess))
                            {
                                return new OperationError("User is not rightsholder in the provided organization", OperationFailure.Forbidden);
                            }
                            subQueries.Add(new QueryByRightsHolder(rightsHolderUuid.Value));
                        }

                        return Result<IQueryable<ItInterface>, OperationError>.Success(_itInterfaceService.GetAvailableInterfaces(subQueries.ToArray()));
                    }
                );
        }

        public Result<ItInterface, OperationError> UpdateItInterface(Guid interfaceUuid, RightsHolderItInterfaceUpdateParameters updateParameters)
        {
            if (updateParameters == null)
                throw new ArgumentNullException(nameof(updateParameters));

            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            try
            {
                var exposingSystem = _systemService.GetSystem(updateParameters.ExposingSystemUuid);

                if (exposingSystem.Failed)
                {
                    if (exposingSystem.Error.FailureType == OperationFailure.NotFound) //If we can't find the exposing system the call will never work and should return BadInput.
                        return new OperationError("Invalid exposing system id provided", OperationFailure.BadInput);
                    return exposingSystem.Error;
                }

                var result = _itInterfaceService
                    .GetInterface(interfaceUuid)
                    .Bind(WithRightsHolderAccessTo)
                    .Bind(WithActiveEntityOnly)
                    .Bind(itInterface => _itInterfaceService.UpdateNameAndInterfaceId(itInterface.Id, updateParameters.Name, updateParameters.InterfaceId))
                    .Bind(ItInterface => ApplyUpdates(ItInterface, updateParameters, exposingSystem.Value.Id));

                if (result.Ok)
                {
                    transaction.Commit();
                }
                else
                {
                    _logger.Error("Failed to update It-Interface with uuid: {uuid} due to error: {errorMessage}", interfaceUuid, result.Error.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed updating rightsholder It-Interface with uuid {uuid}", interfaceUuid);
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        private Result<ItInterface, OperationError> ApplyUpdates(ItInterface itInterface, IRightsHolderWriteableItInterfaceParameters updates, int exposingSystemId)
        {
            return _itInterfaceService.UpdateExposingSystem(itInterface.Id, exposingSystemId)
                .Bind(itInterface => _itInterfaceService.UpdateVersion(itInterface.Id, updates.Version))
                .Bind(itInterface => _itInterfaceService.UpdateDescription(itInterface.Id, updates.Description))
                .Bind(itInterface => _itInterfaceService.UpdateUrlReference(itInterface.Id, updates.UrlReference));
        }

    }
}
