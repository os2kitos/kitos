using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices
{
    public interface IItSystemService
    {
        IEnumerable<ItSystem> GetSystems(int organizationId, string nameSearch, User user);
        IEnumerable<ItSystem> GetNonInterfaces(int organizationId, string nameSearch, User user);
        IEnumerable<ItSystem> GetInterfaces(int organizationId, string nameSearch, User user);
        IEnumerable<ItSystem> GetHierarchy(int systemId);
    }
}