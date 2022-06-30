using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Contracts;

namespace Core.ApplicationServices.Contract
{
    public interface IItContractOptionsApplicationService
    {
        Result<ContractOptions, OperationError> GetAssignableContractOptions(int organizationId);
    }
}
