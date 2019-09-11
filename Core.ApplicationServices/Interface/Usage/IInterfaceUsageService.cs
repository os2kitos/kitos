using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Interface.Usage
{
    public interface IInterfaceUsageService
    {
        Result<OperationResult, ItInterfaceUsage> Create(int systemUsageId, int systemId, int interfaceId);
        Result<OperationResult, ItInterfaceUsage> Update(
            object[] key, 
            int? contractId, 
            int? infrastructureId, 
            bool isWishedFor);

        Result<OperationResult, object> DeleteByKey(object[] key);
    }
}
