using Core.Abstractions.Types;
using Core.ApplicationServices.Model.System;
using Core.DomainModel.ItSystem;
using System;

namespace Core.ApplicationServices.System.Write
{
    public interface IItSystemWriteService
    {
        Result<ItSystem, OperationError> CreateNewSystem(Guid organizationUuid, SystemUpdateParameters parameters);
        Result<ItSystem, OperationError> Update(Guid systemUuid, SystemUpdateParameters parameters);
        Result<ItSystem, OperationError> Delete(Guid systemUuid);
    }
}
