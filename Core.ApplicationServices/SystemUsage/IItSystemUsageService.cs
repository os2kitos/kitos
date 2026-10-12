using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.ApplicationServices.SystemUsage
{
    public interface IItSystemUsageService
    {
        IQueryable<ItSystemUsage> Query(params IDomainQuery<ItSystemUsage>[] conditions);
        Result<ItSystemUsage, OperationError> CreateNew(int itSystemId, int organizationId);
        Result<ItSystemUsage, OperationError> Delete(int id);
        ItSystemUsage? GetByOrganizationAndSystemId(int organizationId, int systemId);
        ItSystemUsage? GetById(int usageId);
        Result<ItSystemUsage, OperationError> GetItSystemUsageByUuidAndAuthorizeRead(Guid uuid);
        Result<ItSystemUsage, OperationError> GetItSystemUsageByUuid(Guid uuid);
        Result<CombinedPermissionsResult, OperationError> GetPermissions(Guid uuid);
        Result<ResourceCollectionPermissionsResult, OperationError> GetCollectionPermissions(Guid organizationUuid);
        
        Result<ArchivePeriod, OperationError> RemoveArchivePeriod(int systemUsageId,Guid archivePeriodUuid);
        Result<IEnumerable<ArchivePeriod>, OperationError> RemoveAllArchivePeriods(int systemUsageId);
        Result<ArchivePeriod, OperationError> AddArchivePeriod(int systemUsageId, DateTime startDate, DateTime endDate, string archiveId, bool approved);
        Result<ArchivePeriod, OperationError> UpdateArchivePeriod(int systemUsageId, Guid archivePeriodUuid, DateTime startDate, DateTime endDate, string archiveId, bool approved);
        Maybe<OperationError> TransferResponsibleUsage(int systemId, Guid targetUnitUuid);
        Maybe<OperationError> TransferRelevantUsage(int systemId, Guid unitUuid, Guid targetUnitUuid);
        Maybe<OperationError> RemoveResponsibleUsage(int id);
        Maybe<OperationError> RemoveRelevantUnit(int id, Guid unitUuid);
        Maybe<OperationError> AddPersonalDataOption(int id, GDPRPersonalDataOption option);
        Maybe<OperationError> RemovePersonalDataOption(int id, GDPRPersonalDataOption option);
    }
}