using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.DomainModel.ItContract;


namespace Core.ApplicationServices.Contract.Write
{
    public interface IItContractWriteService
    {
        Result<ItContract, OperationError> Create(Guid organizationUuid, ItContractModificationParameters parameters);
        Result<ItContract, OperationError> Update(Guid itContractUuid, ItContractModificationParameters parameters);
        Maybe<OperationError> Delete(Guid itContractUuid);
    }
}
