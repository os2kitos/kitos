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

        Maybe<OperationError> DeleteSystemRelation(Guid itSystemUsageUuid, Guid itSystemUsageRelationUuid);
    }
}
