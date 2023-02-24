using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.DataAccess;
using Serilog;
using Core.DomainModel.Events;

namespace Core.ApplicationServices.Interface.Write
{
    public class ItInterfaceWriteService : IItInterfaceWriteService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IItSystemService _systemService;
        private readonly IItInterfaceService _itInterfaceService;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IDatabaseControl _databaseControl;
        private readonly IDomainEvents _domainEvents;

        public ItInterfaceWriteService(
            IOrganizationRepository organizationRepository,
            IItSystemService systemService,
            IItInterfaceService itInterfaceService,
            ITransactionManager transactionManager,
            ILogger logger,
            IAuthorizationContext authorizationContext,
            IDatabaseControl databaseControl,
            IDomainEvents domainEvents)
        {
            _organizationRepository = organizationRepository;
            _itInterfaceService = itInterfaceService;
            _transactionManager = transactionManager;
            _systemService = systemService;
            _logger = logger;
            _authorizationContext = authorizationContext;
            _databaseControl = databaseControl;
            _domainEvents = domainEvents;
        }

        private Result<ItInterface, OperationError> ApplyUpdates(ItInterface originalInterface, ItInterfaceWriteModel update)
        {
            return originalInterface.WithOptionalUpdate(update.Version, (itInterface, newVersion) => _itInterfaceService.UpdateVersion(itInterface.Id, newVersion))
                .Bind(itInterface => UpdateNameAndInterfaceId(itInterface, update))
                .Bind(itInterface => itInterface.WithOptionalUpdate(update.ExposingSystemUuid, UpdateExposingSystem))
                .Bind(itInterface => itInterface.WithOptionalUpdate(update.Description, (interfaceToUpdate, newDescription) => _itInterfaceService.UpdateDescription(interfaceToUpdate.Id, newDescription)))
                .Bind(itInterface => itInterface.WithOptionalUpdate(update.UrlReference, (interfaceToUpdate, newUrlReference) => _itInterfaceService.UpdateUrlReference(interfaceToUpdate.Id, newUrlReference)))
                .Bind(itInterface => itInterface.WithOptionalUpdate(update.Deactivated, UpdateActivatedState));
        }

        /*
         *public OptionalValueChange<AccessModifier> Scope { get; set; } = OptionalValueChange<AccessModifier>.None;
        public OptionalValueChange<Guid?> InterfaceTypeUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<string> Note { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<ItInterfaceDataWriteModel> Data { get; set; } = OptionalValueChange<ItInterfaceDataWriteModel>.None;
         *
         */

        private Result<ItInterface, OperationError> UpdateActivatedState(ItInterface interfaceToUpdate, bool deactivated)
        {
            return deactivated ? _itInterfaceService.Deactivate(interfaceToUpdate.Id) : _itInterfaceService.Activate(interfaceToUpdate.Id);
        }

        private Result<ItInterface, OperationError> UpdateNameAndInterfaceId(ItInterface itInterface, ItInterfaceWriteModel update)
        {
            if (update.Name.IsUnchanged && update.InterfaceId.IsUnchanged)
                return itInterface; //No changes found

            var newName = update.Name.MapOptionalChangeWithFallback(itInterface.Name);
            var newInterfaceId = update.InterfaceId.MapOptionalChangeWithFallback(itInterface.ItInterfaceId);

            return _itInterfaceService.UpdateNameAndInterfaceId(itInterface.Id, newName, newInterfaceId);
        }

        private Result<ItInterface, OperationError> UpdateExposingSystem(ItInterface itInterface, Guid exposingSystemUuid)
        {
            var exposingSystem = _systemService.GetSystem(exposingSystemUuid);

            if (exposingSystem.Failed)
            {
                return exposingSystem.Error.FailureType == OperationFailure.NotFound
                    ? new OperationError("Invalid exposing system id provided", OperationFailure.BadInput)
                    : exposingSystem.Error;
            }

            return _itInterfaceService.UpdateExposingSystem(itInterface.Id, exposingSystem.Value.Id);
        }

        public Result<ItInterface, OperationError> Create(Guid organizationUuid, ItInterfaceWriteModel parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var name = parameters.Name;
            if (name.IsUnchanged)
                return new OperationError("Name must be provided", OperationFailure.BadInput);

            using var transaction = _transactionManager.Begin();
            try
            {
                var organizationId = _organizationRepository.GetByUuid(organizationUuid).Select(x => x.Id);

                if (organizationId.IsNone)
                    return new OperationError("Invalid org id provided", OperationFailure.BadInput);

                var interfaceId = parameters.InterfaceId;

                //Remove changes for any values used during the creation process
                parameters.Name = OptionalValueChange<string>.None;
                parameters.InterfaceId = OptionalValueChange<string>.None;

                var result = _itInterfaceService
                    .CreateNewItInterface(organizationId.Value, name.NewValue, interfaceId.MapOptionalChangeWithFallback(string.Empty))
                    .Bind(itInterface => ApplyUpdates(itInterface, parameters));

                if (result.Ok)
                {
                    SaveAndNotify(result.Value, transaction);
                }
                else
                {
                    transaction.Rollback();
                    _logger.Error("failed to create It-Interface {name} due to error: {errorMessage}", name.NewValue, result.Error.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed creating It-Interface");
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        public Result<ItInterface, OperationError> Update(Guid interfaceUuid, ItInterfaceWriteModel parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            using var transaction = _transactionManager.Begin();
            try
            {
                var result = GetItInterfaceAndAuthorizeAccess(interfaceUuid)
                    .Bind(itInterface => ApplyUpdates(itInterface, parameters));

                if (result.Ok)
                {
                    SaveAndNotify(result.Value, transaction);
                }
                else
                {
                    transaction.Rollback();
                    _logger.Error("Failed to update It-Interface with uuid: {uuid} due to error: {errorMessage}", interfaceUuid, result.Error.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed updating It-Interface with uuid {uuid}", interfaceUuid);
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        public Result<ItInterface, OperationError> Delete(Guid interfaceUuid)
        {
            try
            {
                return GetItInterfaceAndAuthorizeAccess(interfaceUuid)
                    .Select(itInterface => _itInterfaceService.Delete(itInterface.Id, true))
                    .Match
                    (deleteResult => deleteResult.Match<Result<ItInterface, OperationError>>
                        (
                            deletedInterface => deletedInterface,
                            failure => new OperationError(failure)
                        )
                        , error => error
                    );
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed deleting It-Interface with uuid {uuid}", interfaceUuid);
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        private Result<ItInterface, OperationError> WithWriteAccess(ItInterface itInterface)
        {
            return _authorizationContext.AllowModify(itInterface) ? itInterface : new OperationError(OperationFailure.Forbidden);
        }

        private void SaveAndNotify(ItInterface itInterface, IDatabaseTransaction transaction)
        {
            _domainEvents.Raise(new EntityUpdatedEvent<ItInterface>(itInterface));
            _databaseControl.SaveChanges();
            transaction.Commit();
        }
        private Result<ItInterface, OperationError> GetItInterfaceAndAuthorizeAccess(Guid interfaceUuid)
        {
            return _itInterfaceService
                .GetInterface(interfaceUuid)
                .Bind(WithWriteAccess);
        }
    }
}
