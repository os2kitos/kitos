﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries.SystemUsage;
using Core.DomainServices.Queries;
using Presentation.Web.Extensions;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;

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
            string systemNameContent = null,
            DateTime? changedSinceGtEq = null,
            BoundedPaginationQuery paginationQuery = null)
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
                .OrderByDefaultConventions(changedSinceGtEq.HasValue)
                .Page(paginationQuery).AsEnumerable();
        }
    }
}