using System.Collections.Generic;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.System
{
    public interface IReferenceService
    {
        TwoTrackResult<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemId(int systemId);
    }
}
