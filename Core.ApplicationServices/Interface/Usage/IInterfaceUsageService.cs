using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Interface.Usage
{
    public interface IInterfaceUsageService
    {
        Result<OperationResult, ItInterfaceUsage> AssociateInContract(
            int systemUsageId, 
            int systemId, 
            int interfaceId,
            int? contractId, 
            int? infrastructureId,
            bool isWishedFor);

        OperationResult Delete(int systemUsageId, int systemId, int interfaceId);
    }
}
