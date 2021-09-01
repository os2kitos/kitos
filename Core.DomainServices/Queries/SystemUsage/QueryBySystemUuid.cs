using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices.Queries.SystemUsage
{
    public class QueryBySystemUuid : IDomainQuery<ItSystemUsage>
    {
        private readonly Guid _systemUuid;

        public QueryBySystemUuid(Guid systemUuid)
        {
            _systemUuid = systemUuid;
        }

        public IQueryable<ItSystemUsage> Apply(IQueryable<ItSystemUsage> source)
        {
            return source.Where(systemUsage => systemUsage.ItSystem.Uuid == _systemUuid);
        }
    }
}
