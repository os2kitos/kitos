using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.DomainModel.ItSystemUsage;


namespace Core.ApplicationServices.SystemUsage.Write
{
    public interface IItSystemUsageWriteService
    {
        Result<ItSystemUsage, OperationError> Create(SystemUsageCreationParameters parameters);
        Result<ItSystemUsage, OperationError> Update(Guid systemUsageUuid, SystemUsageUpdateParameters parameters);
        Result<ItSystemUsage, OperationError> AddRole(Guid systemUsageUuid, UserRolePair parameters);
        Result<ItSystemUsage, OperationError> RemoveRole(Guid systemUsageUuid, UserRolePair parameters);
        Maybe<OperationError> Delete(Guid itSystemUsageUuid);
        Result<SystemRelation, OperationError> CreateSystemRelation(Guid fromSystemUsageUuid, SystemRelationParameters parameters);
        Result<SystemRelation, OperationError> UpdateSystemRelation(Guid fromSystemUsageUuid, Guid relationUuid, SystemRelationParameters parameters);
        Maybe<OperationError> DeleteSystemRelation(Guid itSystemUsageUuid, Guid itSystemUsageRelationUuid);
    }
}
