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


        public IEnumerable<ItSystem> GetSystems(Organization organization, string nameSearch)
        {
            //TODO filter by organization or public

            if (nameSearch == null) return _repository.Get();

            return _repository.Get(system => system.Name.Contains(nameSearch));
        }

        public IEnumerable<ItSystem> GetNonInterfaces(Organization organization, string nameSearch)
        {
            return GetSystems(organization, nameSearch).Where(system => system.AppType.Id != InterfaceAppType.Id);
        }

        public IEnumerable<ItSystem> GetInterfaces(Organization organization, string nameSearch)
        {
            return GetSystems(organization, nameSearch).Where(system => system.AppType.Id == InterfaceAppType.Id);
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