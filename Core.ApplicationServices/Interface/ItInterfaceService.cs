using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.OptionTypes;
using Core.ApplicationServices.Organizations;
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
        private readonly IOptionResolver _optionResolver;
        private readonly IOrganizationService _organizationService;

        public ItInterfaceService(
            IGenericRepository<DataRow> dataRowRepository,
            IItSystemRepository systemRepository,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IInterfaceRepository interfaceRepository,
            IOrganizationalUserContext userContext,
            IOperationClock operationClock,
            IOptionResolver optionResolver,
            IOrganizationService organizationService)
        {
            _dataRowRepository = dataRowRepository;
            _systemRepository = systemRepository;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _interfaceRepository = interfaceRepository;
            _userContext = userContext;
            _operationClock = operationClock;
            _optionResolver = optionResolver;
            _organizationService = organizationService;
        }

        public Result<ItInterface, OperationFailure> Delete(int id, bool breakBindings = false)
        {
            using var transaction = _transactionManager.Begin();
            var getItInterfaceResult = _interfaceRepository.GetInterface(id);

            if (getItInterfaceResult.IsNone)
            {
                return OperationFailure.NotFound;
            }

            var itInterface = getItInterfaceResult.Value;
            var permissions = GetPermissions(itInterface);
            if (permissions.Failed)
            {
                return permissions.Error.FailureType;
            }

            var itInterfacePermissions = permissions.Value;
            if (!itInterfacePermissions.BasePermissions.Delete)
            {
                return OperationFailure.Forbidden;
            }

            var conflicts = itInterfacePermissions.DeletionConflicts.ToList();
            if (conflicts.Contains(ItInterfaceDeletionConflict.ExposedByItSystem))
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

        public IQueryable<ItInterface> GetAvailableInterfaces(params IDomainQuery<ItInterface>[] conditions)
        {
            var accessLevel = _authorizationContext.GetCrossOrganizationReadAccess();

            var refinement = GetQueryRefinement(accessLevel);

            var mainQuery = _interfaceRepository.GetInterfaces();

            var refinedResult = refinement
                .Select(x => x.Apply(mainQuery))
                .GetValueOrFallback(mainQuery);

            return conditions.Any() ? new IntersectionQuery<ItInterface>(conditions).Apply(refinedResult) : refinedResult;
        }

        private Maybe<IDomainQuery<ItInterface>> GetQueryRefinement(CrossOrganizationDataReadAccessLevel accessLevel)
        {
            if (_userContext.HasStakeHolderAccess())
            {
                return Maybe<IDomainQuery<ItInterface>>.None;
            }
            if (accessLevel == CrossOrganizationDataReadAccessLevel.RightsHolder)
            {
                var rightsHolderOrgs = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess);
                return new QueryByRightsHolderIdsOrOwnOrganizationIds(rightsHolderOrgs, _userContext.OrganizationIds);
            }
            return new QueryAllByRestrictionCapabilities<ItInterface>(accessLevel, _userContext.OrganizationIds);
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

        public Result<ItInterface, OperationError> Activate(int id)
        {
            return Mutate(id,
                itInterface => itInterface.Disabled,
                itInterface =>
                {
                    itInterface.Activate();
                    _domainEvents.Raise(new EnabledStatusChanged<ItInterface>(itInterface, true, false));
                });
        }

        public Result<ItInterface, OperationError> UpdateNote(int id, string newValue)
        {
            return Mutate(id,
                itInterface => itInterface.Note != newValue,
                itInterface =>
                {
                    itInterface.Note = newValue;
                });
        }

        public Result<ItInterface, OperationError> UpdateAccessModifier(int id, AccessModifier newValue)
        {
            return Mutate(id,
                itInterface => itInterface.AccessModifier != newValue,
                updateWithResult: itInterface =>
                {
                    if (_authorizationContext.HasPermission(new VisibilityControlPermission(itInterface)))
                    {
                        itInterface.AccessModifier = newValue;
                        return itInterface;
                    }
                    return new OperationError(OperationFailure.Forbidden);
                });
        }

        public Result<ItInterface, OperationError> UpdateInterfaceType(int id, Guid? interfaceTypeUuid)
        {
            return Mutate(id,
                itInterface => itInterface.Interface?.Uuid != interfaceTypeUuid,
                updateWithResult: itInterface =>
                {
                    if (interfaceTypeUuid.HasValue)
                    {
                        var result = _optionResolver.GetOptionType<ItInterface, InterfaceType>(itInterface.Organization.Uuid, interfaceTypeUuid.Value);
                        if (result.Failed)
                        {
                            return new OperationError(
                                $"Failed to resolve interface type:{result.Error.Message.GetValueOrFallback("no_message")}", result.Error.FailureType);
                        }
                        var (option, available) = result.Value;
                        if (!available)
                        {
                            return new OperationError($"Cannot assign unavailable InterfaceType option with uuid:{option.Uuid:D}", OperationFailure.BadInput);
                        }

                        itInterface.Interface = option;
                    }
                    else
                    {
                        itInterface.Interface = null;
                    }

                    return itInterface;
                });
        }

        public Result<ItInterface, OperationError> ReplaceInterfaceData(int id, IEnumerable<ItInterfaceDataWriteModel> newData)
        {
            return Mutate(id, _ => true, updateWithResult: itInterface => ReplaceInterfaceData(itInterface, newData));
        }

        private Result<ItInterface, OperationError> ReplaceInterfaceData(ItInterface itInterface, IEnumerable<ItInterfaceDataWriteModel> newData)
        {
            //Clear existing data
            if (itInterface.DataRows.Any())
            {
                var existingRows = itInterface.DataRows.ToList();
                itInterface.DataRows.Clear();
                _dataRowRepository.RemoveRange(existingRows);
            }

            //Add replacement data
            foreach (var newRowData in newData)
            {
                DataType dataType = null;
                if (newRowData.DataTypeUuid.HasValue)
                {
                    var availableOptionResult = LoadAvailableDataTypeOption(itInterface.Organization.Uuid, newRowData.DataTypeUuid.Value);
                    if (availableOptionResult.Failed)
                    {
                        return availableOptionResult.Error;
                    }

                    dataType = availableOptionResult.Value;
                }

                itInterface.AddDataRow(newRowData.DataDescription, dataType.FromNullable());
            }

            return itInterface;
        }

        public Result<ItInterfacePermissions, OperationError> GetPermissions(Guid uuid)
        {
            return GetInterface(uuid).Transform(GetPermissions);
        }

        private Result<ItInterfacePermissions, OperationError> GetPermissions(Result<ItInterface, OperationError> result)
        {
            return ResourcePermissionsResult
                .FromResolutionResult(result, _authorizationContext)
                .Select(permissions => new ItInterfacePermissions(permissions, GetDeletionConflicts(result, permissions.Delete)));
        }

        private static IEnumerable<ItInterfaceDeletionConflict> GetDeletionConflicts(Result<ItInterface, OperationError> itInterface, bool allowDelete)
        {
            return allowDelete
                ? itInterface.Select(GetDeletionConflicts).Match(conflicts => conflicts, _ => Array.Empty<ItInterfaceDeletionConflict>())
                : Array.Empty<ItInterfaceDeletionConflict>();
        }

        private static IEnumerable<ItInterfaceDeletionConflict> GetDeletionConflicts(ItInterface arg)
        {
            if (arg.ExhibitedBy != null)
                yield return ItInterfaceDeletionConflict.ExposedByItSystem;
        }

        public Result<ResourceCollectionPermissionsResult, OperationError> GetCollectionPermissions(Guid organizationUuid)
        {
            return _organizationService
                .GetOrganization(organizationUuid)
                .Select(organization => ResourceCollectionPermissionsResult.FromOrganizationId<ItInterface>(organization.Id, _authorizationContext));
        }

        public Result<DataRow, OperationError> AddInterfaceData(int id, ItInterfaceDataWriteModel parameters)
        {
            DataRow created = null;
            return Mutate(id, _ => true, updateWithResult: itInterface =>
            {
                DataType dataType = null;
                if (parameters.DataTypeUuid.HasValue)
                {
                    var dataTypeResult = LoadAvailableDataTypeOption(itInterface.Organization.Uuid, parameters.DataTypeUuid.Value);
                    if (dataTypeResult.Failed)
                        return dataTypeResult.Error;
                    dataType = dataTypeResult.Value;
                }

                created = itInterface.AddDataRow(parameters.DataDescription, dataType.FromNullable());
                return itInterface;
            }).Match(_ => Result<DataRow, OperationError>.Success(created), error => error);
        }

        public Result<DataRow, OperationError> UpdateInterfaceData(int id, Guid dataUuid, ItInterfaceDataWriteModel parameters)
        {
            DataRow updated = null;
            return Mutate(id, _ => true, updateWithResult: itInterface =>
            {
                var dataRowResult = itInterface.GetDataRow(dataUuid);
                if (dataRowResult.IsNone)
                    return new OperationError("Invalid " + nameof(dataUuid), OperationFailure.BadInput);

                updated = dataRowResult.Value;
                updated.Data = parameters.DataDescription;

                //If changed, update the data type option
                if (updated.DataType?.Uuid != parameters.DataTypeUuid && parameters.DataTypeUuid.HasValue)
                {
                    var dataTypeResult = LoadAvailableDataTypeOption(itInterface.Organization.Uuid, parameters.DataTypeUuid.Value);
                    if (dataTypeResult.Failed)
                        return dataTypeResult.Error;
                    updated.DataType = dataTypeResult.Value;
                }
                else if (parameters.DataTypeUuid == null)
                {
                    updated.ResetDataType();
                }
                return itInterface;
            }).Match(_ => Result<DataRow, OperationError>.Success(updated), error => error);
        }

        public Result<DataRow, OperationError> DeleteInterfaceData(int id, Guid dataUuid)
        {
            DataRow deleted = null;
            return Mutate(id, _ => true, updateWithResult: itInterface =>
            {
                var dataRowResult = itInterface.GetDataRow(dataUuid);
                if (dataRowResult.IsNone)
                    return new OperationError("Invalid " + nameof(dataUuid), OperationFailure.BadInput);
                deleted = dataRowResult.Value;
                itInterface.DataRows.Remove(dataRowResult.Value);
                _dataRowRepository.Delete(deleted);
                return itInterface;
            }).Match(_ => Result<DataRow, OperationError>.Success(deleted), error => error);
        }

        private static bool ValidateName(string name)
        {
            return string.IsNullOrWhiteSpace(name) == false &&
                   name.Length <= ItInterface.MaxNameLength;
        }

        private static bool ValidateItInterfaceId(string itInterfaceId)
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

        private Result<DataType, OperationError> LoadAvailableDataTypeOption(Guid organizationUuid, Guid optionUuid)
        {
            var optionResult = _optionResolver.GetOptionType<DataRow, DataType>(organizationUuid, optionUuid);
            if (optionResult.Failed)
            {
                return optionResult.Error;
            }

            var option = optionResult.Value;
            if (!option.available)
            {
                return new OperationError(
                    $"Selected data type with uuid {optionUuid} is not available in the organization",
                    OperationFailure.BadInput);
            }

            return option.option;
        }
    }
}
