using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.SystemUsage.Write
{
    public interface IItSystemUsageWriteService
    {
        Result<ItSystemUsage, OperationError> Create(SystemUsageCreationParameters parameters);
        Result<ItSystemUsage, OperationError> Update(Guid systemUsageUuid, SystemUsageUpdateParameters parameters);
        Result<ItSystemUsage, OperationError> AddRole(Guid systemUsageUuid, UserRolePair assignment);
        Result<ItSystemUsage, OperationError> RemoveRole(Guid systemUsageUuid, UserRolePair assignment);
        Maybe<OperationError> Delete(Guid itSystemUsageUuid);
        Result<SystemRelation, OperationError> CreateSystemRelation(Guid fromSystemUsageUuid, SystemRelationParameters parameters);
        Result<SystemRelation, OperationError> UpdateSystemRelation(Guid fromSystemUsageUuid, Guid relationUuid, SystemRelationParameters parameters);
        Maybe<OperationError> DeleteSystemRelation(Guid itSystemUsageUuid, Guid itSystemUsageRelationUuid);
        Result<ArchivePeriod, OperationError> CreateJournalPeriod(Guid systemUsageUuid, SystemUsageJournalPeriodProperties parameters);
        Result<ArchivePeriod, OperationError> UpdateJournalPeriod(Guid systemUsageUuid, Guid relationUuid, SystemUsageJournalPeriodProperties parameters);
        Maybe<OperationError> DeleteJournalPeriod(Guid systemUsageUuid, Guid relationUuid);
    }
}
