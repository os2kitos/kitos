using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Interface.Usage
{
    public interface IInterfaceUsageService
    {
        Result<OperationResult, ItInterfaceUsage> Create(
            int systemUsageId, 
            int systemId, 
            int interfaceId,
            bool isWishedFor,
            int contractId,
            int? infrastructureId = null);

        OperationResult Delete(int systemUsageId, int systemId, int interfaceId);
    }
}
