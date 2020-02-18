using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.References;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.References
{
    public interface IReferenceService
    {
        Result<ExternalReference, OperationError> Create(int referenceOwnerId, ReferenceRootType referenceOwnerType, string title, string externalReferenceId, string url, Display display);
        Result<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemId(int systemId);
    }
}
