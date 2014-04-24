using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices
{
    public interface IItSystemService
    {
        AppType InterfaceAppType { get; }

        IEnumerable<ItSystem> GetAllSystems(Organization organization);
        IEnumerable<ItSystem> GetAllNonInterfaces(Organization organization);
        IEnumerable<ItSystem> GetAllInterfaces(Organization organization);
    }
}