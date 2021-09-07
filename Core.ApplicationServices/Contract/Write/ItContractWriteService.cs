using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.DomainModel.ItContract;


namespace Core.ApplicationServices.Contract.Write
{
    public class ItContractWriteService : IItContractWriteService
    {
        public Result<ItContract, OperationError> Create(Guid organizationUuid, ItContractModificationParameters parameters)
        {
            throw new NotImplementedException();
        }

        public Result<ItContract, OperationError> Update(Guid itContractUuid, ItContractModificationParameters parameters)
        {
            throw new NotImplementedException();
        }

        public Maybe<OperationError> Delete(Guid itContractUuid)
        {
            throw new NotImplementedException();
        }
    }
}
