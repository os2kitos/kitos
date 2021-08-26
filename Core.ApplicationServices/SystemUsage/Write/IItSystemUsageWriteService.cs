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

        Result<SystemRelation, OperationError> CreateSystemRelation(Guid fromSystemUsageUuid, SystemRelationParameters parameters);
        Result<SystemRelation, OperationError> UpdateSystemRelation(Guid fromSystemUsageUuid, Guid relationUuid, SystemRelationParameters parameters);
        Maybe<OperationError> DeleteSystemRelation(Guid itSystemUsageUuid, Guid itSystemUsageRelationUuid);
    }
}
