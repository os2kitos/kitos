using System;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.SystemUsage.Write
{
    public interface IItSystemUsageWriteService
    {
        Result<ItSystemUsage, OperationError> Create(SystemUsageCreationParameters parameters);
        Result<ItSystemUsage, OperationError> Update(Guid systemUsageUuid, SystemUsageUpdateParameters parameters);

        Maybe<OperationError> Delete(Guid itSystemUsageUuid);
        //TODO: Add the Maybe<Value<T>> In the contract types and add modification methods for it
        //TODO: Relations delegate to the relations service but exposes more or less the same interface (except for all of the GET methods used by ui).
    }
}
