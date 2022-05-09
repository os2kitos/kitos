﻿using System;
using System.Data;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Interface;
using Core.DomainServices.Repositories.Interface;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;

using DataRow = Core.DomainModel.ItSystem.DataRow;

namespace Core.ApplicationServices.Interface
{
    public class ItInterfaceService : IItInterfaceService
    {
        private readonly IGenericRepository<DataRow> _dataRowRepository;
        private readonly IItSystemRepository _systemRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IInterfaceRepository _interfaceRepository;
        private readonly IOrganizationalUserContext _userContext;
        private readonly IOperationClock _operationClock;

        public ItInterfaceService(
            IGenericRepository<DataRow> dataRowRepository,
            IItSystemRepository systemRepository,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IInterfaceRepository interfaceRepository,
            IOrganizationalUserContext userContext,
            IOperationClock operationClock)
        {
            _dataRowRepository = dataRowRepository;
            _systemRepository = systemRepository;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _interfaceRepository = interfaceRepository;
            _userContext = userContext;
            _operationClock = operationClock;
        }
        public Result<ItInterface, OperationFailure> Delete(int id, bool breakBindings = false)
        {
            using (var transaction = _transactionManager.Begin())
            {
                var getItInterfaceResult = _interfaceRepository.GetInterface(id);

                if (getItInterfaceResult.IsNone)
                {
                    return OperationFailure.NotFound;
                }

                var itInterface = getItInterfaceResult.Value;
                if (!_authorizationContext.AllowDelete(itInterface))
                {
                    return OperationFailure.Forbidden;
                }

                if (itInterface.ExhibitedBy != null)
                {
                    if (breakBindings)
                    {
                        var updateExposingSystemResult = UpdateExposingSystem(id, null);
                        if (updateExposingSystemResult.Failed)
                            return updateExposingSystemResult.Error.FailureType;
                    }
                    else
                    {
                        return OperationFailure.Conflict;
                    }
                }

                var dataRows = itInterface.DataRows.ToList();
                foreach (var dataRow in dataRows)
                {
                    _dataRowRepository.DeleteByKey(dataRow.Id);
                }
                _dataRowRepository.Save();

                // delete it interface
                _domainEvents.Raise(new EntityBeingDeletedEvent<ItInterface>(itInterface));
                _interfaceRepository.Delete(itInterface);

                transaction.Commit();
                return itInterface;
            }
        }

        public IQueryable<ItInterface> GetAvailableInterfaces(params IDomainQuery<ItInterface>[] conditions)
        {
            var accessLevel = _authorizationContext.GetCrossOrganizationReadAccess();
            var refinement = Maybe<IDomainQuery<ItInterface>>.None;

            if (accessLevel == CrossOrganizationDataReadAccessLevel.RightsHolder)
            {
                var rightsHolderOrgs = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess);
                refinement = new QueryByRightsHolderIdsOrOwnOrganizationIds(rightsHolderOrgs, _userContext.OrganizationIds);
            }
            else
            {
                refinement = new QueryAllByRestrictionCapabilities<ItInterface>(accessLevel, _userContext.OrganizationIds);
            }

            var mainQuery = _interfaceRepository.GetInterfaces();

            var refinedResult = refinement
                .Select(x => x.Apply(mainQuery))
                .GetValueOrFallback(mainQuery);

            return conditions.Any() ? new IntersectionQuery<ItInterface>(conditions).Apply(refinedResult) : refinedResult;
        }

        public Result<ItInterface, OperationError> GetInterface(Guid uuid)
        {
            return _interfaceRepository
                .GetInterface(uuid)
                .Match
                (
                    itInterface => _authorizationContext.AllowReads(itInterface) ? Result<ItInterface, OperationError>.Success(itInterface) : new OperationError(OperationFailure.Forbidden),
                    () => new OperationError(OperationFailure.NotFound)
                );
        }

        public Result<ItInterface, OperationError> CreateNewItInterface(int organizationId, string name, string interfaceId, Guid? rightsHolderProvidedUuid = null, AccessModifier? accessModifier = null)
        {
            if (_authorizationContext.AllowCreate<ItInterface>(organizationId))
            {
                using var transaction = _transactionManager.Begin();

                var nameError = CheckForUniqueNaming(name, interfaceId, organizationId);

                if (nameError.HasValue)
                    return nameError.Value;

                var uuidTaken = rightsHolderProvidedUuid?.Transform(uuid => _interfaceRepository.GetInterface(uuid).HasValue) == true;

                if (uuidTaken)
                    return new OperationError("UUID already exists on another it-interface in KITOS", OperationFailure.Conflict);

                var newItInterface = new ItInterface
                {
                    OrganizationId = organizationId,
                    Uuid = rightsHolderProvidedUuid ?? Guid.NewGuid(),
                    ItInterfaceId = interfaceId,
                    ObjectOwnerId = _userContext.UserId,
                    Name = name,
                    Created = _operationClock.Now
                };

                if (accessModifier != null)
                {
                    if (!AllowCreationWithAccessLevel(organizationId, accessModifier.Value, newItInterface))
                    {
                        return new OperationError("Cannot create interface with the provided access level", OperationFailure.Forbidden);
                    }
                }
                else
                {
                    accessModifier = AllowCreationWithAccessLevel(organizationId, AccessModifier.Public, newItInterface)
                        ? AccessModifier.Public
                        : AccessModifier.Local;
                }

                newItInterface.AccessModifier = accessModifier.Value;

                _interfaceRepository.Add(newItInterface);
                _domainEvents.Raise(new EntityCreatedEvent<ItInterface>(newItInterface));
                transaction.Commit();
                return newItInterface;
            }
            return new OperationError(OperationFailure.Forbidden);
        }

        private bool AllowCreationWithAccessLevel(int organizationId, AccessModifier accessModifier, ItInterface newItInterface)
        {
            return _authorizationContext.HasPermission(new CreateEntityWithVisibilityPermission(accessModifier, newItInterface, organizationId));
        }

        public Result<ItInterface, OperationError> UpdateNameAndInterfaceId(int id, string name, string itInterfaceId)
        {
            return Mutate(id,
                itInterface => itInterface.ItInterfaceId != itInterfaceId || itInterface.Name != name,
                updateWithResult: itInterface =>
                {
                    var nameError = CheckForUniqueNaming(name, itInterfaceId, itInterface.OrganizationId);
                    if (nameError.HasValue)
                        return nameError.Value;

                    itInterface.Name = name;
                    itInterface.ItInterfaceId = itInterfaceId;
                    return itInterface;
                });
        }

        public Result<ItInterface, OperationError> UpdateVersion(int id, string newValue)
        {
            return Mutate(id, itInterface => itInterface.Version != newValue, itInterface => itInterface.Version = newValue);
        }

        public Result<ItInterface, OperationError> UpdateDescription(int id, string newValue)
        {
            return Mutate(id, itInterface => itInterface.Description != newValue, itInterface => itInterface.Description = newValue);
        }

        public Result<ItInterface, OperationError> UpdateUrlReference(int id, string newValue)
        {
            return Mutate(id, itInterface => itInterface.Url != newValue, itInterface => itInterface.Url = newValue);
        }

        public Result<ItInterface, OperationError> UpdateExposingSystem(int id, int? newSystemId)
        {
            return Mutate(id,
                itInterface => itInterface.ExhibitedBy?.ItSystemId != newSystemId,
                updateWithResult: itInterface =>
                {

                    Maybe<ItSystem> oldSystem = itInterface.ExhibitedBy?.ItSystem;
                    var newSystem = Maybe<ItSystem>.None;

                    if (newSystemId.HasValue)
                    {
                        newSystem = _systemRepository.GetSystem(newSystemId.Value).FromNullable();
                        if (newSystem.IsNone)
                            return new OperationError($"Cannot set ItSystem with id: {newSystemId.Value} as exposing system for ItInterface with id: {id} since the system does not exist", OperationFailure.BadInput);

                        if (!_authorizationContext.AllowReads(newSystem.Value))
                            return new OperationError(OperationFailure.Forbidden);
                    }

                    var newExhibit = itInterface.ChangeExhibitingSystem(newSystem);

                    _domainEvents.Raise(new ExposingSystemChanged(itInterface, oldSystem, newExhibit.Select(x => x.ItSystem)));
                    if (oldSystem.HasValue)
                    {
                        _domainEvents.Raise(new EntityUpdatedEvent<ItSystem>(oldSystem.Value));
                    }
                    if (newSystem.HasValue)
                    {
                        _domainEvents.Raise(new EntityUpdatedEvent<ItSystem>(newSystem.Value));
                    }

                    return itInterface;
                });
        }

        public Result<ItInterface, OperationError> Deactivate(int id)
        {
            return Mutate(id,
                itInterface => itInterface.Disabled == false,
                itInterface =>
            {
                itInterface.Deactivate();
                _domainEvents.Raise(new EnabledStatusChanged<ItInterface>(itInterface, false, true));
            });
        }

        private bool ValidateName(string name)
        {
            return string.IsNullOrWhiteSpace(name) == false &&
                   name.Length <= ItInterface.MaxNameLength;
        }

        private bool ValidateItInterfaceId(string itInterfaceId)
        {
            return itInterfaceId != null;
        }

        private Maybe<OperationError> CheckForUniqueNaming(string name, string itInterfaceId, int orgId)
        {
            if (!ValidateName(name))
                return new OperationError("Name was not valid", OperationFailure.BadInput);

            if (!ValidateItInterfaceId(itInterfaceId))
                return new OperationError("ItInterfaceId was not valid", OperationFailure.BadInput);

            if (_interfaceRepository
                    .GetInterfaces()
                    .ByOrganizationId(orgId)
                    .ByNameExact(name)
                    .Where(x => x.ItInterfaceId == itInterfaceId)
                    .Any())
                return new OperationError("Name and ItInterfaceId combination already exists in the organization", OperationFailure.Conflict);


            return Maybe<OperationError>.None;
        }

        private Result<ItInterface, OperationError> Mutate(int interfaceId, Predicate<ItInterface> performUpdateTo, Action<ItInterface> updateWith = null, Func<ItInterface, Result<ItInterface, OperationError>> updateWithResult = null)
        {
            if (updateWith == null && updateWithResult == null)
                throw new ArgumentException("No mutations provided");

            using var transaction = _transactionManager.Begin();

            var getItInterfaceResult = _interfaceRepository.GetInterface(interfaceId);
            if (getItInterfaceResult.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var itInterface = getItInterfaceResult.Value;
            if (!_authorizationContext.AllowModify(itInterface))
                return new OperationError(OperationFailure.Forbidden);

            if (performUpdateTo(itInterface))
            {
                updateWith?.Invoke(itInterface);
                var result = updateWithResult?.Invoke(itInterface) ?? Result<ItInterface, OperationError>.Success(itInterface);
                if (result.Ok)
                {
                    _domainEvents.Raise(new EntityUpdatedEvent<ItInterface>(itInterface));
                    _interfaceRepository.Update(itInterface);
                    transaction.Commit();
                }
                else
                {
                    //Terminate the flow
                    return result;
                }
            }
            return itInterface;
        }
    }
}
