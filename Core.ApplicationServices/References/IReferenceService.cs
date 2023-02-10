using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared.Write;
using Core.DomainModel;
using Core.DomainModel.References;


namespace Core.ApplicationServices.References
{
    public interface IReferenceService
    {
        Result<ExternalReference, OperationError> AddReference(int rootId, ReferenceRootType rootType, ExternalReferenceProperties externalReferenceProperties);

        Result<ExternalReference, OperationError> UpdateReference(int rootId, ReferenceRootType rootType, Guid referenceUuid, ExternalReferenceProperties externalReferenceProperties);
        Result<ExternalReference, OperationFailure> DeleteByReferenceId(int referenceId);
        Result<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemId(int systemId);
        Result<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemUsageId(int usageId);
        Result<IEnumerable<ExternalReference>, OperationFailure> DeleteByContractId(int contractId);
        Result<IEnumerable<ExternalReference>, OperationFailure> DeleteByDataProcessingRegistrationId(int id);
        Maybe<OperationError> UpdateExternalReferences(ReferenceRootType rootType, int rootId, IEnumerable<UpdatedExternalReferenceProperties> externalReferences);
    }
}
