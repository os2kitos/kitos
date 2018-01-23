using Core.DomainModel;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices
{
    public interface IItSystemUsageService
    {
        ItSystemUsage Add(ItSystemUsage ItSystemUsage, User objectOwner);
        //void AddInterfaceUsage(ItSystemUsage usage, ItSystem theInterface);
        void Delete(int id);
    }
}