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
            int? contractId, //TODO: not nullable (see method name)
            int? infrastructureId, //TODO: Move as last param and assign default value
            bool isWishedFor);

        //Result<OperationResult, ItInterfaceUsage> AssociateInContract(
        //    int systemUsageId,
        //    int systemId,
        //    int interfaceId,
        //    bool isWishedFor,
        //    int? contractId = null,
        //    int? infrastructureId = null);

        OperationResult Delete(int systemUsageId, int systemId, int interfaceId);
    }
}
