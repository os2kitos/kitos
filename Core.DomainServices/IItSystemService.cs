using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices
{
    public interface IItSystemService
    {
        AppType InterfaceAppType { get; }

        IEnumerable<ItSystem> GetSystems(int organizationId, string nameSearch);
        IEnumerable<ItSystem> GetNonInterfaces(int organizationId, string nameSearch);
        IEnumerable<ItSystem> GetInterfaces(int organizationId, string nameSearch);
        IEnumerable<ItSystem> GetHierarchy(int systemId);
    }
}