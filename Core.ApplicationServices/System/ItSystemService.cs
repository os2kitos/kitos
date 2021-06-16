using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.ItSystem;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Serilog;

namespace Core.ApplicationServices.System
{
    public class ItSystemService : IItSystemService
    {
        private readonly IGenericRepository<ItSystem> _repository;
        private readonly IItSystemRepository _itSystemRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IReferenceService _referenceService;
        private readonly ILogger _logger;
        private readonly IOrganizationalUserContext _userContext;
        private readonly IDomainEvents _domainEvents;

        public ItSystemService(
            IGenericRepository<ItSystem> repository,
            IItSystemRepository itSystemRepository,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IReferenceService referenceService,
            ILogger logger,
            IOrganizationalUserContext userContext,
            IDomainEvents domainEvents
            )
        {
            _repository = repository;
            _itSystemRepository = itSystemRepository;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _referenceService = referenceService;
            _logger = logger;
            _userContext = userContext;
            _domainEvents = domainEvents;
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
            var system = _repository.GetByKey(systemId);
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

        public SystemDeleteResult Delete(int id)
        {
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
                return SystemDeleteResult.InUse;
            }

            if (system.Children.Any())
            {
                return SystemDeleteResult.HasChildren;
            }

            if (system.ItInterfaceExhibits.Any())
            {
                return SystemDeleteResult.HasInterfaceExhibits;
            }

            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                try
                {
                    var deleteReferenceResult = _referenceService.DeleteBySystemId(system.Id);
                    if (deleteReferenceResult.Ok == false)
                    {
                        _logger.Error($"Failed to delete external references of it system with id: {system.Id}. Service returned a {deleteReferenceResult.Error}");
                        transaction.Rollback();
                        return SystemDeleteResult.UnknownError;
                    }
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
        }

        public Result<ItSystem, OperationError> CreateNewSystem(int organizationId, string name, Maybe<Guid> uuid)
        {
            if (_authorizationContext.AllowCreate<ItSystem>(organizationId))
            {
                using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

                var nameError = ValidateNewSystemName(organizationId, name);

                if (nameError.HasValue)
                    return nameError.Value;

                var uuidTaken = uuid.Select(id => _itSystemRepository.GetSystem(id).HasValue).GetValueOrFallback(false);

                if (uuidTaken)
                    return new OperationError("UUID already exists on another it-system in KITOS", OperationFailure.Conflict);

                var newSystem = new ItSystem
                {
                    OrganizationId = organizationId,
                    AccessModifier = AccessModifier.Public,
                    Uuid = uuid.Match(providedUuid => providedUuid, Guid.NewGuid),
                    ObjectOwnerId = _userContext.UserId
                };

                _itSystemRepository.Add(newSystem);
                _domainEvents.Raise(new EntityCreatedEvent<ItSystem>(newSystem));
                transaction.Commit();
                return newSystem;
            }

            return new OperationError(OperationFailure.Forbidden);
        }

        public bool CanChangeNameTo(int organizationId, int systemId, string newName)
        {
            return ValidateName(newName) &&
                   FindSystemsByNameInOrganization(organizationId, newName).ExceptEntitiesWithIds(systemId).Any() == false;
        }

        private static bool ValidateName(string newName)
        {
            return string.IsNullOrWhiteSpace(newName) == false &&
                   newName.Length <= ItSystem.MaxNameLength;
        }

        public bool CanCreateSystemWithName(int organizationId, string name)
        {
            return ValidateNewSystemName(organizationId, name).IsNone;
        }

        public Maybe<OperationError> ValidateNewSystemName(int organizationId, string name)
        {
            if (!ValidateName(name))
                return new OperationError("Name was not valid", OperationFailure.BadInput);

            if (FindSystemsByNameInOrganization(organizationId, name).Any())
                return new OperationError("Name already exists within the target organization", OperationFailure.Conflict);

            return Maybe<OperationError>.None;
        }

        private IQueryable<ItSystem> FindSystemsByNameInOrganization(int organizationId, string name)
        {
            return _repository
                .AsQueryable()
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
    }
}
