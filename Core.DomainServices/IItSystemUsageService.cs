using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices
{
    public interface IItSystemUsageService
    {
        ItSystemUsage Add(int systemId, int orgId, User owner);

        InterfaceUsage AddInterfaceUsage(int systemUsageId, ItSystem theInterface, bool isDefault = false);
        InterfaceUsage AddInterfaceUsage(ItSystemUsage systemUsage, ItSystem theInterface, bool isDefault = false);
    }
}