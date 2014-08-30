using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class ItSystemService : IItSystemService
    {
        private readonly IGenericRepository<ItSystem> _repository;

        public ItSystemService(IGenericRepository<ItSystem> repository, IGenericRepository<AppType> appTypeRepository)
        {
            _repository = repository;
            InterfaceAppType = appTypeRepository.Get(appType => appType.Name == "Snitflade").Single();
        }

        public AppType InterfaceAppType { get; private set; }


        public IEnumerable<ItSystem> GetSystems(int organizationId, string nameSearch)
        {
            if (nameSearch == null) return _repository.Get(x => x.OrganizationId == organizationId || x.AccessModifier == AccessModifier.Public);

            return _repository.Get(system => system.Name.Contains(nameSearch) && system.OrganizationId == organizationId || system.AccessModifier == AccessModifier.Public);
        }

        public IEnumerable<ItSystem> GetNonInterfaces(int organizationId, string nameSearch)
        {
            return GetSystems(organizationId, nameSearch).Where(system => system.AppType == null || system.AppType.Id != InterfaceAppType.Id);
        }

        public IEnumerable<ItSystem> GetInterfaces(int organizationId, string nameSearch)
        {
            return GetSystems(organizationId, nameSearch).Where(system => system.AppType != null && system.AppType.Id == InterfaceAppType.Id);
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
    }
}