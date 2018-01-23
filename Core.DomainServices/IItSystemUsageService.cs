using Core.DomainModel;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices
{
    public interface IItSystemUsageService
    {
        ItSystemUsage Add(int systemId, int orgId, DataSensitivityLevel dataLevel, User objectOwner);
        //void AddInterfaceUsage(ItSystemUsage usage, ItSystem theInterface);
        void Delete(int id);
    }
}