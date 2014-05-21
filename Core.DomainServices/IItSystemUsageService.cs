using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices
{
    public interface IItSystemUsageService
    {
        ItSystemUsage Add(int systemId, int orgId, User owner);

        void AddInterfaceUsage(ItSystemUsage systemUsage, int interfaceId);
    }
}