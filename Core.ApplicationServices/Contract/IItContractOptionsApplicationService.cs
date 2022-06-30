using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Contracts;

namespace Core.ApplicationServices.Contract
{
    //TODO: Not needed
    public interface IItContractOptionsApplicationService
    {
        //TODO: Move to IItcontractService - also remember to include "availability" in the result sop you can generate NamedEntityWithExpirationStatusDTO
        // In the frontend, use that info to determine which to provide in the "select" and which to just use for lookup... if option is not available, add "udgået"
        // In the iitcontract service, use the IOptionsService<TReference, TOption> to get the available options along with the availability status
        Result<ContractOptions, OperationError> GetAssignableContractOptions(int organizationId); 

    }
}
