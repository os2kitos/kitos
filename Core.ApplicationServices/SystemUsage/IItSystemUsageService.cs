using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainServices.Queries;

namespace Core.ApplicationServices.SystemUsage
{
    public interface IItSystemUsageService
    {
        IQueryable<ItSystemUsage> Query(params IDomainQuery<ItSystemUsage>[] conditions);
        Result<ItSystemUsage, OperationError> CreateNew(int itSystemId, int organizationId);
        Result<ItSystemUsage, OperationError> Delete(int id);
        ItSystemUsage GetByOrganizationAndSystemId(int organizationId, int systemId);
        ItSystemUsage GetById(int usageId);
        Result<ItSystemUsage, OperationError> GetReadableItSystemUsageByUuid(Guid uuid);
        Result<ResourcePermissionsResult, OperationError> GetPermissions(Guid uuid);

        /// <summary>
        /// Adds information about which data sensitivity levels are applied to the system usage />
        /// </summary>
        /// <param name="itSystemUsageId"></param>
        /// <param name="sensitiveDataLevel"></param>
        /// <returns></returns>
        Result<ItSystemUsageSensitiveDataLevel, OperationError> AddSensitiveDataLevel(int itSystemUsageId, SensitiveDataLevel sensitiveDataLevel);

        /// <summary>
        /// Removes information about which data sensitivity levels are applied to the system usage />
        /// </summary>
        /// <param name="itSystemUsageId"></param>
        /// <param name="sensitiveDataLevel"></param>
        /// <returns></returns>
        Result<ItSystemUsageSensitiveDataLevel, OperationError> RemoveSensitiveDataLevel(int itSystemUsageId, SensitiveDataLevel sensitiveDataLevel);

        Result<ArchivePeriod, OperationError> RemoveArchivePeriod(int systemUsageId,Guid archivePeriodUuid);
        Result<IEnumerable<ArchivePeriod>, OperationError> RemoveAllArchivePeriods(int systemUsageId);
        Result<ArchivePeriod, OperationError> AddArchivePeriod(int systemUsageId, DateTime startDate, DateTime endDate, string archiveId, bool approved);
        Result<ArchivePeriod, OperationError> UpdateArchivePeriod(int systemUsageId, Guid archivePeriodUuid, DateTime startDate, DateTime endDate, string archiveId, bool approved);
        Result<ItSystemUsage, OperationError> GetItSystemUsageById(int usageId);
        Maybe<OperationError> TransferResponsibleUsage(int systemId, Guid targetUnitUuid);
        Maybe<OperationError> TransferRelevantUsage(int systemId, Guid unitUuid, Guid targetUnitUuid);
        Maybe<OperationError> RemoveResponsibleUsage(int id);
        Maybe<OperationError> RemoveRelevantUnit(int id, Guid unitUuid);
        Maybe<OperationError> AddPersonalDataOption(int id, GDPRPersonalDataOption option);
        Maybe<OperationError> RemovePersonalDataOption(int id, GDPRPersonalDataOption option);
    }
}