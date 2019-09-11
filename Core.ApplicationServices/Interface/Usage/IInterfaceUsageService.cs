using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Interface.Usage
{
    public interface IInterfaceUsageService
    {
        ////TODO:AssociateInContract(alle parametre der kræves ved oprettelse)

        Result<OperationResult, ItInterfaceUsage> Create(int systemUsageId, int systemId, int interfaceId);

        //TODO:Not needed
        Result<OperationResult, ItInterfaceUsage> Update(
            object[] key, 
            int? contractId, 
            int? infrastructureId, 
            bool isWishedFor);

        //TODO:Delete(int systemUsageId, int systemId, int interfaceId) - returns OperationResult
        Result<OperationResult, object> DeleteByKey(object[] key);
    }
}
