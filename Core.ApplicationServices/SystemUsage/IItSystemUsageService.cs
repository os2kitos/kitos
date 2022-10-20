using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
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
        Result<ItSystemUsage,OperationError> GetByUuid(Guid uuid);

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


        Result<IEnumerable<ArchivePeriod>, OperationError> RemoveAllArchivePeriods(int systemUsageId);
        Result<ArchivePeriod, OperationError> AddArchivePeriod(int systemUsageId, DateTime startDate, DateTime endDate, string archiveId, bool approved);
        Result<ItSystemUsage, OperationError> GetItSystemUsageById(int usageId);
        Result<ItSystemUsage, OperationError> TransferResponsibleUsage(int targetUnitId, int id);
        Result<ItSystemUsage, OperationError> TransferRelevantUsage(int unitId, int targetUnitId, int id);
        Result<ItSystemUsage, OperationError> RemoveResponsibleUsage(int id);
        Result<ItSystemUsage, OperationError> RemoveRelevantUnit(int id, int unitId);

        IEnumerable<ItSystemUsage> GetSystemsByResponsibleUnitId(int unitId);
        IEnumerable<ItSystemUsage> GetSystemsByRelevantUnitId(int unitId);
    }
}