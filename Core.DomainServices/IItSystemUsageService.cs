using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices
{
    public interface IItSystemUsageService
    {
        ItSystemUsage Add(int systemId, int orgId, User objectOwner);
        //void AddInterfaceUsage(ItSystemUsage usage, ItSystem theInterface);
    }
}