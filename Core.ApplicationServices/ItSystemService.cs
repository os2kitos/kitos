using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class ItSystemService : IItSystemService
    {
        private readonly IGenericRepository<ItSystem> _repository;

        public ItSystemService(IGenericRepository<ItSystem> repository)
        {
            _repository = repository;
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
                        s.AccessModifier == AccessModifier.Normal &&
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
                    s.AccessModifier == AccessModifier.Normal &&
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
    }
}
