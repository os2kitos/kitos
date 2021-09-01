using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices.Queries.SystemUsage
{
    public class QueryByRelationToSystem : IDomainQuery<ItSystemUsage>
    {
        private readonly Guid _systemUuid;

        public QueryByRelationToSystem(Guid systemUuid)
        {
            _systemUuid = systemUuid;
        }

        public IQueryable<ItSystemUsage> Apply(IQueryable<ItSystemUsage> source)
        {
            return source.Where(systemUsage => systemUsage.UsageRelations.Any(relation => relation.ToSystemUsage.ItSystem.Uuid == _systemUuid));
        }
    }
}
