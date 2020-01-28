using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.System
{
    public interface IReferenceService
    {
        Result<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemId(int systemId);
    }
}
