using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries.SystemUsage;
using Core.DomainServices.Queries;
using Presentation.Web.Extensions;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Types.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Web.Controllers.API.V2.Common.Helpers
{
    /// <summary>
    /// Helper method to consolidate shared query logic supporting both internal and external it-system usage API endpoints
    /// </summary>
    public static class ItSystemUsageV2QueryHandler
    {
        public static IEnumerable<ItSystemUsage> ExecuteItSystemUsagesQuery(
            this IItSystemUsageService service,
            Guid? organizationUuid = null,
            Guid? relatedToSystemUuid = null,
            Guid? relatedToSystemUsageUuid = null,
            Guid? relatedToContractUuid = null,
            Guid? systemUuid = null,
            string? systemNameContent = null,
            DateTime? changedSinceGtEq = null,
            CommonOrderByProperty? orderByProperty = null,
            BoundedPaginationQuery? paginationQuery = null)
        {
            var conditions = new List<IDomainQuery<ItSystemUsage>>();

            if (organizationUuid.HasValue)
                conditions.Add(new QueryByOrganizationUuid<ItSystemUsage>(organizationUuid.Value));

            if (relatedToSystemUuid.HasValue)
                conditions.Add(new QueryByRelationToSystem(relatedToSystemUuid.Value));

            if (relatedToSystemUsageUuid.HasValue)
                conditions.Add(new QueryByRelationToSystemUsage(relatedToSystemUsageUuid.Value));

            if (relatedToContractUuid.HasValue)
                conditions.Add(new QueryByRelationToContract(relatedToContractUuid.Value));

            if (systemUuid.HasValue)
                conditions.Add(new QueryBySystemUuid(systemUuid.Value));

            if (changedSinceGtEq.HasValue)
                conditions.Add(new QueryByChangedSinceGtEq<ItSystemUsage>(changedSinceGtEq.Value));

            if (!string.IsNullOrWhiteSpace(systemNameContent))
                conditions.Add(new QueryBySystemNameContent(systemNameContent));

            return service
                .Query(conditions.ToArray())
                .Include(usage => usage.ItSystem)
                .Include(usage => usage.Organization)
                .Include(usage => usage.ObjectOwner)
                .Include(usage => usage.LastChangedByUser)
                .Include(usage => usage.ExternalReferences)
                .Include(usage => usage.ItSystemCategories)
                .Include(usage => usage.ArchiveLocation)
                .Include(usage => usage.ArchiveTestLocation)
                .Include(usage => usage.ArchiveType)
                .Include(usage => usage.ArchiveSupplier)
                .Include(usage => usage.MainContract)
                    .ThenInclude(mainContract => mainContract.ItContract)
                .Include(usage => usage.ResponsibleUsage)
                    .ThenInclude(responsibleUsage => responsibleUsage.OrganizationUnit)
                .Include(usage => usage.UsedBy)
                    .ThenInclude(usedBy => usedBy.OrganizationUnit)
                .Include(usage => usage.TaskRefs)
                .Include(usage => usage.TaskRefsOptOut)
                .Include(usage => usage.ArchivePeriods)
                .Include(usage => usage.Rights)
                    .ThenInclude(right => right.Role)
                .Include(usage => usage.Rights)
                    .ThenInclude(right => right.User)
                .Include(usage => usage.UsageRelations)
                    .ThenInclude(relation => relation.ToSystemUsage)
                .Include(usage => usage.SensitiveDataLevels)
                .Include(usage => usage.PersonalDataOptions)
                .AsSplitQuery()
                .OrderSystemUsageApiResultsByDefaultConventions(changedSinceGtEq.HasValue, orderByProperty)
                .Page(paginationQuery).ToList();
        }
    }
}
