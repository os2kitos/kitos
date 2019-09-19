using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.System
{
    public class ItSystemService : IItSystemService
    {
        private readonly IGenericRepository<ItSystem> _repository;
        private readonly IGenericRepository<ItSystemRight> _rightsRepository;
        private readonly IItSystemRepository _itSystemRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly ReferenceService _referenceService;
        private readonly ILogger _logger;

        public ItSystemService(
            IGenericRepository<ItSystem> repository, 
            IGenericRepository<ItSystemRight> rightsRepository, 
            IItSystemRepository itSystemRepository,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            ReferenceService referenceService,
            ILogger logger
            )
        {
            _repository = repository;
            _rightsRepository = rightsRepository;
            _itSystemRepository = itSystemRepository;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _referenceService = referenceService;
            _logger = logger;
        }


        public IEnumerable<ItSystem> GetSystems(int organizationId, string nameSearch, User user)
        {
            if (string.IsNullOrWhiteSpace(nameSearch))
                return _repository.Get(
                    s =>
                        // global admin sees all within the context
                        user.IsGlobalAdmin && s.OrganizationId == organizationId ||
                        // object owner sees his own objects
                        s.ObjectOwnerId == user.Id ||
                        // it's public everyone can see it
                        s.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see normal objects
                        s.AccessModifier == AccessModifier.Local &&
                        s.OrganizationId == organizationId
                        // it systems doesn't have roles so private doesn't make sense
                        // only object owners will be albe to see private objects
                    );

            return _repository.Get(
                s =>
                    // filter by name
                    s.Name.Contains(nameSearch) &&
                    // global admin sees all within the context
                    (user.IsGlobalAdmin && s.OrganizationId == organizationId ||
                    // object owner sees his own objects
                    s.ObjectOwnerId == user.Id ||
                    // it's public everyone can see it
                    s.AccessModifier == AccessModifier.Public ||
                    // everyone in the same organization can see normal objects
                    s.AccessModifier == AccessModifier.Local &&
                    s.OrganizationId == organizationId)
                    // it systems doesn't have roles so private doesn't make sense
                    // only object owners will be albe to see private objects
                );
        }

        public IEnumerable<ItSystem> GetNonInterfaces(int organizationId, string nameSearch, User user)
        {
            return GetSystems(organizationId, nameSearch, user);
        }

        public IEnumerable<ItSystem> GetInterfaces(int organizationId, string nameSearch, User user)
        {
            return GetSystems(organizationId, nameSearch, user);
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

        private IEnumerable<ItSystem> GetHierarchyChildren(ItSystem itSystem)
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

        private IEnumerable<ItSystem> GetHierarchyParents(ItSystem itSystem)
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

            if (! _authorizationContext.AllowDelete(system))
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
                return SystemDeleteResult.HasExhibitInterfaces;
            }

            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                try
                {
                    if (system.ExternalReferences.Any())
                    {
                        var ids = system.ExternalReferences.ToList().Select(t => t.Id);
                        _referenceService.Delete(ids);
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

        public IEnumerable<ItSystem> ReportItSystemRights()
        {
            var rights = _rightsRepository.Get();

            return null;
        }

        public Result<OperationResult, IReadOnlyList<UsingOrganization>> GetUsingOrganizations(int systemId)
        {
            var itSystem = _itSystemRepository.GetSystem(systemId);
            if (itSystem == null)
            {
                return Result<OperationResult, IReadOnlyList<UsingOrganization>>.Fail(OperationResult.NotFound);
            }
            if (! _authorizationContext.AllowReads(itSystem))
            {
                return Result<OperationResult, IReadOnlyList<UsingOrganization>>.Fail(OperationResult.Forbidden);
            }

            return Result<OperationResult, IReadOnlyList<UsingOrganization>>.Ok(MapToUsingOrganization(itSystem.Usages));
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

    }
}
