﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.References;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;
using Core.DomainServices.Options;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.ItSystem;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.TaskRefs;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;

using Serilog;

namespace Core.ApplicationServices.System
{
    public class ItSystemService : IItSystemService
    {
        private readonly IItSystemRepository _itSystemRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IReferenceService _referenceService;
        private readonly ITaskRefRepository _taskRefRepository;
        private readonly IOptionsService<ItSystem, BusinessType> _businessTypeService;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ILogger _logger;
        private readonly IOrganizationalUserContext _userContext;
        private readonly IDomainEvents _domainEvents;
        private readonly IOperationClock _operationClock;
        private readonly IItInterfaceService _interfaceService;
        private readonly IItSystemUsageService _systemUsageService;

        public ItSystemService(
            IItSystemRepository itSystemRepository,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IReferenceService referenceService,
            ITaskRefRepository taskRefRepository,
            IOptionsService<ItSystem, BusinessType> businessTypeService,
            IOrganizationRepository organizationRepository,
            ILogger logger,
            IOrganizationalUserContext userContext,
            IDomainEvents domainEvents,
            IOperationClock operationClock,
            IItInterfaceService interfaceService,
            IItSystemUsageService systemUsageService
            )
        {
            _itSystemRepository = itSystemRepository;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _referenceService = referenceService;
            _taskRefRepository = taskRefRepository;
            _businessTypeService = businessTypeService;
            _organizationRepository = organizationRepository;
            _logger = logger;
            _userContext = userContext;
            _domainEvents = domainEvents;
            _operationClock = operationClock;
            _interfaceService = interfaceService;
            _systemUsageService = systemUsageService;
        }

        public Result<ItSystem, OperationError> GetSystem(Guid uuid)
        {
            return _itSystemRepository
                .GetSystem(uuid)
                .Match
                (
                    system => _authorizationContext.AllowReads(system) ? Result<ItSystem, OperationError>.Success(system) : new OperationError(OperationFailure.Forbidden),
                    () => new OperationError(OperationFailure.NotFound)
                );
        }

        public Result<ItSystem, OperationError> GetSystem(int id)
        {
            return _itSystemRepository
                .GetSystem(id)
                .FromNullable()
                .Match
                (
                    system => _authorizationContext.AllowReads(system) ? Result<ItSystem, OperationError>.Success(system) : new OperationError(OperationFailure.Forbidden),
                    () => new OperationError(OperationFailure.NotFound)
                );
        }

        public IQueryable<ItSystem> GetAvailableSystems(params IDomainQuery<ItSystem>[] conditions)
        {
            var accessLevel = _authorizationContext.GetCrossOrganizationReadAccess();
            var refinement = Maybe<IDomainQuery<ItSystem>>.None;

            if (accessLevel == CrossOrganizationDataReadAccessLevel.RightsHolder)
            {
                var rightsHoldingOrganizations = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess);

                refinement = new QueryByRightsHolderIdOrOwnOrganizationIds(rightsHoldingOrganizations, _userContext.OrganizationIds);
            }
            else if (accessLevel < CrossOrganizationDataReadAccessLevel.All)
            {
                refinement = new QueryAllByRestrictionCapabilities<ItSystem>(accessLevel, _userContext.OrganizationIds);
            }

            var mainQuery = _itSystemRepository.GetSystems();

            var refinedResult = refinement
                .Select(x => x.Apply(mainQuery))
                .GetValueOrFallback(mainQuery);

            return conditions.Any() ? new IntersectionQuery<ItSystem>(conditions).Apply(refinedResult) : refinedResult;
        }

        public IQueryable<ItSystem> GetAvailableSystems(int organizationId, string optionalNameSearch = null)
        {
            var itSystems = _itSystemRepository.GetSystems(
                new OrganizationDataQueryParameters(
                    activeOrganizationId: organizationId,
                    breadth: OrganizationDataQueryBreadth.IncludePublicDataFromOtherOrganizations,
                    dataAccessLevel: _authorizationContext.GetDataAccessLevel(organizationId)
                )
            );

            if (!string.IsNullOrWhiteSpace(optionalNameSearch))
            {
                itSystems = itSystems.ByPartOfName(optionalNameSearch);
            }

            return itSystems;
        }

        public IEnumerable<ItSystem> GetHierarchy(int systemId)
        {
            var result = new List<ItSystem>();
            var system = _itSystemRepository.GetSystem(systemId);

            if (system == null)
                throw new ArgumentException("Invalid system id");

            result.Add(system);
            result.AddRange(GetHierarchyChildren(system));
            result.AddRange(GetHierarchyParents(system));

            return result;
        }

        private static IEnumerable<ItSystem> GetHierarchyChildren(ItSystem itSystem)
        {
            var systems = new List<ItSystem>();
            systems.AddRange(itSystem.Children);
            foreach (var child in itSystem.Children)
            {
                var children = GetHierarchyChildren(child);
                systems.AddRange(children);
            }
            return systems;
        }

        private static IEnumerable<ItSystem> GetHierarchyParents(ItSystem itSystem)
        {
            var parents = new List<ItSystem>();
            if (itSystem.Parent != null)
            {
                parents.Add(itSystem.Parent);
                parents.AddRange(GetHierarchyParents(itSystem.Parent));
            }
            return parents;
        }

        public SystemDeleteResult Delete(int id, bool breakBindings = false)
        {
            using var transaction = _transactionManager.Begin();
            var system = _itSystemRepository.GetSystem(id);

            if (system == null)
            {
                return SystemDeleteResult.NotFound;
            }

            if (_authorizationContext.AllowDelete(system) == false)
            {
                return SystemDeleteResult.Forbidden;
            }

            if (system.Usages.Any())
            {
                if (breakBindings)
                {
                    var failedUsageDeletion = system.Usages.ToList().Select(usage=>_systemUsageService.Delete(usage.Id)).FirstOrDefault(x=>x.Failed);
                    if (failedUsageDeletion != null)
                    {
                        _logger.Error("Failed to delete system with id {id} because deleting usages failed", id);
                        return SystemDeleteResult.UnknownError;
                    }
                }
                else
                {
                    return SystemDeleteResult.InUse;
                }
            }

            if (system.Children.Any())
            {
                if (breakBindings)
                {
                    var failedChildDeletion = system
                        .Children
                        .ToList()
                        .Select(child => UpdateParentSystem(child.Id))
                        .FirstOrDefault(result => result.Failed);
                    if (failedChildDeletion != null)
                    {
                        _logger.Error("Failed to delete system with id {id} because deleting children failed", id);
                        return SystemDeleteResult.UnknownError;
                    }
                    system.Children.Clear();
                }
                else
                {
                    return SystemDeleteResult.HasChildren;
                }
            }

            if (system.ItInterfaceExhibits.Any())
            {
                if (breakBindings)
                {
                    var failedUpdate = system
                        .ItInterfaceExhibits
                        .ToList()
                        .Select(exhibit=>_interfaceService.UpdateExposingSystem(exhibit.ItInterface.Id,null))
                        .FirstOrDefault(x=>x.Failed);
                    if (failedUpdate != null)
                    {
                        _logger.Error("Failed to delete system with id {id} because deleting interface exposures failed",id);
                        return SystemDeleteResult.UnknownError;
                    }
                    system.ItInterfaceExhibits.Clear();
                }
                else
                {
                    return SystemDeleteResult.HasInterfaceExhibits;
                }
            }

            try
            {
                var deleteReferenceResult = _referenceService.DeleteBySystemId(system.Id);
                if (deleteReferenceResult.Ok == false)
                {
                    _logger.Error($"Failed to delete external references of it system with id: {system.Id}. Service returned a {deleteReferenceResult.Error}");
                    transaction.Rollback();
                    return SystemDeleteResult.UnknownError;
                }
                _domainEvents.Raise(new EntityBeingDeletedEvent<ItSystem>(system));
                _itSystemRepository.DeleteSystem(system);
                transaction.Commit();
                return SystemDeleteResult.Ok;

            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to delete it system with id: {system.Id}");
                transaction.Rollback();
                return SystemDeleteResult.UnknownError;
            }
        }

        public Result<ItSystem, OperationError> CreateNewSystem(int organizationId, string name, Guid? uuid = null)
        {
            if (_authorizationContext.AllowCreate<ItSystem>(organizationId))
            {
                using var transaction = _transactionManager.Begin();

                var nameError = ValidateNewSystemName(organizationId, name);

                if (nameError.HasValue)
                    return nameError.Value;

                var uuidTaken = uuid?.Transform(id => _itSystemRepository.GetSystem(id).HasValue) == true;

                if (uuidTaken)
                    return new OperationError("UUID already exists on another it-system in KITOS", OperationFailure.Conflict);

                var newSystem = new ItSystem
                {
                    Name = name,
                    OrganizationId = organizationId,
                    AccessModifier = AccessModifier.Public,
                    Uuid = uuid ?? Guid.NewGuid(),
                    ObjectOwnerId = _userContext.UserId,
                    Created = _operationClock.Now
                };

                _itSystemRepository.Add(newSystem);
                _domainEvents.Raise(new EntityCreatedEvent<ItSystem>(newSystem));
                transaction.Commit();
                return newSystem;
            }

            return new OperationError(OperationFailure.Forbidden);
        }

        public Result<ItSystem, OperationError> UpdateName(int systemId, string newName)
        {
            return Mutate(systemId, system => system.Name != newName, updateWithResult: system =>
            {
                return ValidateNameChange(system.OrganizationId, system.Id, newName)
                    .Match
                    (
                        onValue: namingError => namingError,
                        onNone: () => system
                            .UpdateName(newName)
                            .Match
                            (
                                onValue: namingError => namingError,
                                onNone: () => Result<ItSystem, OperationError>.Success(system)
                            )
                    );
            });
        }

        public Result<ItSystem, OperationError> UpdatePreviousName(int systemId, string newPreviousName)
        {
            return Mutate(systemId, system => system.PreviousName != newPreviousName, system => system.PreviousName = newPreviousName);
        }

        public Result<ItSystem, OperationError> UpdateDescription(int systemId, string newDescription)
        {
            return Mutate(systemId, system => system.Description != newDescription, system => system.Description = newDescription);
        }

        public bool CanChangeNameTo(int organizationId, int systemId, string newName)
        {
            return ValidateNameChange(organizationId, systemId, newName).IsNone;
        }

        private Maybe<OperationError> ValidateNameChange(int organizationId, int systemId, string newName)
        {
            if (!ItSystem.IsValidName(newName))
                return new OperationError("Invalid name", OperationFailure.BadInput);

            if (FindSystemsByNameInOrganization(organizationId, newName).ExceptEntitiesWithIds(systemId).Any())
                return new OperationError("Existing system found in the organization with the same name", OperationFailure.Conflict);
            return Maybe<OperationError>.None;
        }

        public bool CanCreateSystemWithName(int organizationId, string name)
        {
            return ValidateNewSystemName(organizationId, name).IsNone;
        }

        public Result<ItSystem, OperationError> UpdateMainUrlReference(int systemId, string urlReference)
        {
            return Mutate(systemId, system => system.Reference?.URL != urlReference, updateWithResult: system =>
            {
                if (string.IsNullOrWhiteSpace(urlReference))
                    return new OperationError("Url must be defined", OperationFailure.BadInput);

                var existingReference = system.Reference;
                if (existingReference != null)
                {
                    existingReference.URL = urlReference;
                    return system;
                }

                var addReferenceResult = _referenceService.AddReference(systemId, ReferenceRootType.System, "Reference", string.Empty, urlReference);
                if (addReferenceResult.Failed)
                {
                    return addReferenceResult.Error;
                }

                return system.SetMasterReference(addReferenceResult.Value).Match(_ => Result<ItSystem, OperationError>.Success(system), error => error);
            });
        }

        public Result<ItSystem, OperationError> UpdateTaskRefs(int systemId, IEnumerable<int> newTaskRefState)
        {
            Predicate<ItSystem> updateIfTaskRefCollectionDiffers = system => system.TaskRefs.Select(x => x.Id).OrderBy(id => id).SequenceEqual(newTaskRefState.OrderBy(id => id)) == false;

            return Mutate(systemId, updateIfTaskRefCollectionDiffers, updateWithResult: system =>
            {
                var inBoundIds = new HashSet<int>(newTaskRefState);
                var taskRefIdsToAdd = inBoundIds.Where(id => system.GetTaskRef(id).IsNone).ToList();
                var taskRefsToRemove = system.TaskRefs.Where(taskRef => !inBoundIds.Contains(taskRef.Id)).ToList();
                var taskRefsToAdd = new List<TaskRef>();
                foreach (var taskRefId in taskRefIdsToAdd)
                {
                    var taskRef = _taskRefRepository.GetTaskRef(taskRefId);
                    if (taskRef.IsNone)
                        return new OperationError("Invalid task id:" + taskRefId, OperationFailure.BadInput);
                    taskRefsToAdd.Add(taskRef.Value);
                }
                taskRefsToRemove.ForEach(system.RemoveTaskRef);
                taskRefsToAdd.ForEach(system.AddTaskRef);
                return system;
            });
        }

        public Result<ItSystem, OperationError> UpdateBusinessType(int systemId, Guid? newBusinessTypeState)
        {
            if (newBusinessTypeState.HasValue)
            {
                var itSystem = _itSystemRepository.GetSystem(systemId);
                if (itSystem == null)
                    return new OperationError(OperationFailure.NotFound);
                var optionByUuid = _businessTypeService.GetOptionByUuid(itSystem.OrganizationId, newBusinessTypeState.Value);

                if (optionByUuid.IsNone)
                    return new OperationError("Business type uuid does not point to a business type in KITOS", OperationFailure.BadInput);

                var option = optionByUuid.Value;

                if (!option.available)
                    return new OperationError("Business exists but is not available in the System's organization", OperationFailure.BadInput);

                return Mutate(systemId, system => system.BusinessTypeId != option.option.Id, system => system.UpdateBusinessType(option.option));
            }

            return Mutate(systemId, system => system.BusinessTypeId != null, system => system.ResetBusinessType());
        }

        public Result<ItSystem, OperationError> UpdateRightsHolder(int systemId, Guid? newRightsHolderState)
        {
            return Mutate(systemId, system => system.BelongsTo?.Uuid != newRightsHolderState, updateWithResult: system =>
            {
                if (newRightsHolderState.HasValue)
                {
                    var rightsHolder = _organizationRepository.GetByUuid(newRightsHolderState.Value);

                    if (rightsHolder.IsNone)
                        return new OperationError("Rightsholder id is invalid", OperationFailure.BadInput);

                    if (!_authorizationContext.AllowReads(rightsHolder.Value))
                        return new OperationError("Rightsholder organization is not accessible", OperationFailure.Forbidden);

                    system.UpdateRightsHolder(rightsHolder.Value);
                }
                else
                {
                    system.ResetRightsHolder();
                }

                return system;
            });
        }

        public Result<ItSystem, OperationError> UpdateParentSystem(int systemId, int? newParentSystemState = null)
        {
            return Mutate(systemId, system => system.ParentId != newParentSystemState, updateWithResult: system =>
            {
                if (newParentSystemState.HasValue)
                {
                    var parent = _itSystemRepository.GetSystem(newParentSystemState.Value);

                    if (parent == null)
                        return new OperationError("Parent system id is invalid", OperationFailure.BadInput);

                    if (!_authorizationContext.AllowReads(parent))
                        return new OperationError("Access to parent system is denied", OperationFailure.Forbidden);

                    system.SetUpdateParentSystem(parent);
                }
                else
                {
                    system.ResetParentSystem();
                }

                return system;
            });
        }

        public Result<ItSystem, OperationError> Deactivate(int systemId)
        {
            return Mutate(systemId, system => system.Disabled == false, system =>
            {
                system.Deactivate();
                _domainEvents.Raise(new EnabledStatusChanged<ItSystem>(system, false, true));
            });
        }

        public Maybe<OperationError> ValidateNewSystemName(int organizationId, string name)
        {
            if (!ItSystem.IsValidName(name))
                return new OperationError("Name was not valid", OperationFailure.BadInput);

            if (FindSystemsByNameInOrganization(organizationId, name).Any())
                return new OperationError("Name already exists within the target organization", OperationFailure.Conflict);

            return Maybe<OperationError>.None;
        }

        private IQueryable<ItSystem> FindSystemsByNameInOrganization(int organizationId, string name)
        {
            return _itSystemRepository.GetSystems()
                .ByOrganizationId(organizationId)
                .ByNameExact(name);
        }


        public Result<IReadOnlyList<UsingOrganization>, OperationFailure> GetUsingOrganizations(int systemId)
        {
            var itSystem = _itSystemRepository.GetSystem(systemId);
            if (itSystem == null)
            {
                return OperationFailure.NotFound;
            }
            if (!_authorizationContext.AllowReads(itSystem))
            {
                return OperationFailure.Forbidden;
            }

            return MapToUsingOrganization(itSystem.Usages).ToList();
        }

        private static IEnumerable<UsingOrganization> MapToUsingOrganization(IEnumerable<ItSystemUsage> itSystemUsages)
        {
            return itSystemUsages.Select(
                itSystemUsage => new UsingOrganization(
                    itSystemUsage.Id,
                    new NamedEntity(
                        itSystemUsage.Organization.Id,
                        itSystemUsage.Organization.Name)))
                .ToList()
                .AsReadOnly();
        }
        private Result<ItSystem, OperationError> Mutate(int systemId, Predicate<ItSystem> performUpdateTo, Action<ItSystem> updateWith = null, Func<ItSystem, Result<ItSystem, OperationError>> updateWithResult = null)
        {
            if (updateWith == null && updateWithResult == null)
                throw new ArgumentException("No mutations provided");

            using var transaction = _transactionManager.Begin();

            var itSystem = _itSystemRepository.GetSystem(systemId);
            if (itSystem == null)
                return new OperationError(OperationFailure.NotFound);

            if (!_authorizationContext.AllowModify(itSystem))
                return new OperationError(OperationFailure.Forbidden);

            if (performUpdateTo(itSystem))
            {
                updateWith?.Invoke(itSystem);
                var result = updateWithResult?.Invoke(itSystem) ?? Result<ItSystem, OperationError>.Success(itSystem);
                if (result.Ok)
                {
                    _domainEvents.Raise(new EntityUpdatedEvent<ItSystem>(itSystem));
                    _itSystemRepository.Update(itSystem);
                    transaction.Commit();
                }
                else
                {
                    //Terminate the flow
                    return result;
                }
            }

            return itSystem;
        }
    }
}
