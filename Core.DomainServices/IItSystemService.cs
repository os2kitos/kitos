using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices
{
    public interface IItSystemService
    {
        AppType InterfaceAppType { get; }

        IEnumerable<ItSystem> GetSystems(Organization organization, string nameSearch);
        IEnumerable<ItSystem> GetNonInterfaces(Organization organization, string nameSearch);
        IEnumerable<ItSystem> GetInterfaces(Organization organization, string nameSearch);
        IEnumerable<ItSystem> GetHierarchy(int systemId);
    }
}