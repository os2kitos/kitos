using Core.DomainModel.ItContract;
using System;
using System.Linq;

namespace Core.DomainServices.Queries.Contract
{
    public class QueryBySystemUuid : IDomainQuery<ItContract>
    {
        private readonly Guid _systemUuid;

        public QueryBySystemUuid(Guid systemUuid)
        {
            _systemUuid = systemUuid;
        }

        public IQueryable<ItContract> Apply(IQueryable<ItContract> source)
        {
            return source.Where(x => x.AssociatedSystemUsages.Any(x => x.ItSystemUsage != null && x.ItSystemUsage.ItSystem != null && x.ItSystemUsage.ItSystem.Uuid == _systemUuid));
        }
    }
}
