using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared.Write;
using Core.DomainModel;
using Core.DomainModel.ItContract;


namespace Core.ApplicationServices.Contract.Write
{
    public interface IItContractWriteService
    {
        Result<ItContract, OperationError> Create(Guid organizationUuid, ItContractModificationParameters parameters);
        Result<ItContract, OperationError> Update(Guid itContractUuid, ItContractModificationParameters parameters);
        Maybe<OperationError> Delete(Guid itContractUuid);
        Maybe<OperationError> DeleteContractWithChildren(Guid itContractUuid);
        Maybe<OperationError> TransferContracts(Guid? parentUuid, IEnumerable<Guid> itContractUuids);
        Result<ExternalReference, OperationError> AddExternalReference(Guid contractUuid,
            ExternalReferenceProperties externalReferenceProperties);
        Result<ExternalReference, OperationError> UpdateExternalReference(Guid contractUuid, Guid externalReferenceUuid,
            ExternalReferenceProperties externalReferenceProperties);
        Result<ExternalReference, OperationError>
            DeleteExternalReference(Guid contractUuid, Guid externalReferenceUuid);

        Result<ItContract, OperationError> AddRole(Guid contractUuid, UserRolePair assignment);
        Result<ItContract, OperationError> RemoveRole(Guid systemUsageUuid, UserRolePair assignment);
    }
}
