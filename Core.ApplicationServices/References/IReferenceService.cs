using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.References;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.References
{
    public interface IReferenceService
    {
        Result<ExternalReference, OperationError> AddReference(int rootId, ReferenceRootType rootType, string title, string externalReferenceId, string url, Display display);
        Result<ExternalReference, OperationFailure> DeleteByReferenceId(int referenceId);
        Result<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemId(int systemId);
        Result<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemUsageId(int usageId);
        Result<IEnumerable<ExternalReference>, OperationFailure> DeleteByContractId(int contractId);
        Result<IEnumerable<ExternalReference>, OperationFailure> DeleteByProjectId(int projectId);
        Result<IEnumerable<ExternalReference>, OperationFailure> DeleteByDataProcessingRegistrationId(int id);
    }
}
