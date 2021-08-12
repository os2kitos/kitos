using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.SystemUsage;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.SystemUsage
{
    public interface IItSystemUsageService
    {
        IQueryable<ItSystemUsage> Query(params IDomainQuery<ItSystemUsage>[] conditions);

        Result<ItSystemUsage, OperationFailure> Add(ItSystemUsage usage);
        Result<ItSystemUsage, OperationFailure> Delete(int id);
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
    }
}