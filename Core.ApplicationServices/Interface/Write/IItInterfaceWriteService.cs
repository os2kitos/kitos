using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Interface;
using Core.DomainModel.ItSystem;

namespace Core.ApplicationServices.Interface.Write
{
    public interface IItInterfaceWriteService
    {
        Result<ItInterface, OperationError> Create(Guid organizationUuid, ItInterfaceWriteModel parameters);
        Result<ItInterface, OperationError> Update(Guid interfaceUuid, ItInterfaceWriteModel parameters);
        Result<ItInterface, OperationError> Delete(Guid interfaceUuid);
    }
}
