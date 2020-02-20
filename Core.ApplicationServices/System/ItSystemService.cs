using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;
using Core.DomainServices.Model.Result;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
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

        public ItSystemService(
            IGenericRepository<ItSystem> repository,
            IItSystemRepository itSystemRepository,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IReferenceService referenceService,
            ILogger logger
            )
        {
            _repository = repository;
            _itSystemRepository = itSystemRepository;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _referenceService = referenceService;
            _logger = logger;
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

        public Result<IReadOnlyList<UsingOrganization>, OperationFailure> GetUsingOrganizations(int systemId)
        {
            var itSystem = _itSystemRepository.GetSystem(systemId);
            if (itSystem == null)
            {
                return Result<IReadOnlyList<UsingOrganization>, OperationFailure>.Failure(OperationFailure.NotFound);
            }
            if (!_authorizationContext.AllowReads(itSystem))
            {
                return Result<IReadOnlyList<UsingOrganization>, OperationFailure>.Failure(OperationFailure.Forbidden);
            }

            return Result<IReadOnlyList<UsingOrganization>, OperationFailure>.Success(SortedMapToUsingOrganization(itSystem.Usages));
        }

        private static IReadOnlyList<UsingOrganization> MapToUsingOrganization(IEnumerable<ItSystemUsage> itSystemUsages)
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

        private static IReadOnlyList<UsingOrganization> SortedMapToUsingOrganization(IEnumerable<ItSystemUsage> itSystemUsages)
        {
            return itSystemUsages.Select(
                    itSystemUsage => new UsingOrganization(
                        itSystemUsage.Id,
                        new NamedEntity(
                            itSystemUsage.Organization.Id,
                            itSystemUsage.Organization.Name))).OrderBy(item => item.Organization.Name)
                .ToList()
                .AsReadOnly();
        }

    }
}
