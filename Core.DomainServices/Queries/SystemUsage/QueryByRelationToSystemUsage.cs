using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices.Queries.SystemUsage
{
    public class QueryByRelationToSystemUsage : IDomainQuery<ItSystemUsage>
    {
        private readonly Guid _systemUsageUuid;

        public QueryByRelationToSystemUsage(Guid systemUsageUuid)
        {
            _systemUsageUuid = systemUsageUuid;
        }

        public IQueryable<ItSystemUsage> Apply(IQueryable<ItSystemUsage> source)
        {
            return source.Where(systemUsage => systemUsage.UsageRelations.Any(relation => relation.ToSystemUsage.Uuid == _systemUsageUuid));
        }
    }
}
