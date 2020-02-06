using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.Interface.Usage
{
    public interface IInterfaceUsageService
    {
        Result<ItInterfaceUsage, OperationFailure> Create(
            int systemUsageId,
            int systemId,
            int interfaceId,
            bool isWishedFor,
            int contractId,
            int? infrastructureId = null);

        Result<ItInterfaceUsage, OperationFailure> Delete(int systemUsageId, int systemId, int interfaceId);
    }
}
