using System.Collections.Generic;
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

namespace Core.ApplicationServices.ItSystemServices
{
    public class ItSystemService : IItSystemService
    {
        private readonly IGenericRepository<ItSystem> _repository;
        private readonly IGenericRepository<ItSystemRight> _rightsRepository;
        private readonly IItSystemRepository _itSystemRepository;
        private readonly IAuthorizationContext _authorizationContext;

        public ItSystemService(
            IGenericRepository<ItSystem> repository, 
            IGenericRepository<ItSystemRight> rightsRepository, 
            IItSystemRepository itSystemRepository,
            IAuthorizationContext authorizationContext)
        {
            _repository = repository;
            _rightsRepository = rightsRepository;
            _itSystemRepository = itSystemRepository;
            _authorizationContext = authorizationContext;
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

        public void Delete(int id)
        {
            // http://stackoverflow.com/questions/15226312/entityframewok-how-to-configure-cascade-delete-to-nullify-foreign-keys
            // when children are loaded into memory the foreign key is correctly set to null on children when deleted
            var system = _repository.Get(x => x.Id == id, null, $"{nameof(ItSystem.TaskRefs)}").FirstOrDefault();

            // delete it project
            _repository.Delete(system);
            _repository.Save();
        }

        public IEnumerable<ItSystem> ReportItSystemRights()
        {
            var rights = _rightsRepository.Get();

            return null;
        }

        public Result<OperationResult, IReadOnlyList<UsingOrganization>> GetUsingOrganizations(int systemId)
        {
            var itSystem = _itSystemRepository.GetSystem(systemId);
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
                        itSystemUsage.Organization.Name))
                ).ToList().AsReadOnly();
        }

    }
}
