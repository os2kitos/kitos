using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices
{
    public interface IItSystemUsageService
    {
        ItSystemUsage Add(int systemId, int orgId, User objectOwner);
        void AddInterfaceUsage(ItSystemUsage usage, ItSystem theInterface);
    }
}