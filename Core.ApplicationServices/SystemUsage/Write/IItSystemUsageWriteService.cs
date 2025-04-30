using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.DomainModel.ItSystem;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using System.Collections.Generic;

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
        Result<ArchivePeriod, OperationError> UpdateJournalPeriod(Guid systemUsageUuid, Guid periodUuid, SystemUsageJournalPeriodProperties parameters);
        Result<ArchivePeriod, OperationError> DeleteJournalPeriod(Guid systemUsageUuid, Guid periodUuid);
        Result<ExternalReference, OperationError> AddExternalReference(Guid usageUuid, ExternalReferenceProperties externalReferenceProperties);
        Result<ExternalReference, OperationError> UpdateExternalReference(Guid usageUuid, Guid externalReferenceUuid, ExternalReferenceProperties externalReferenceProperties);
        Result<ExternalReference, OperationError> DeleteExternalReference(Guid usageUuid, Guid externalReferenceUuid);
        Maybe<OperationError> DeleteByItSystemAndOrganizationUuids(Guid itSystemUuid, Guid organizationUuid);
    }
}
