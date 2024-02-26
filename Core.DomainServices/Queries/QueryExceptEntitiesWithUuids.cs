using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryExceptEntitiesWithUuids<T> : IDomainQuery<T>
        where T : class, IHasUuid
    {
        private readonly IReadOnlyList<Guid> _uuids;

        public QueryExceptEntitiesWithUuids(IEnumerable<Guid> uuids)
        {
            _uuids = uuids.ToList().AsReadOnly();
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            //If not ids, return source query. If any return the filtered query
            return _uuids.Any() ? source.Where(x => _uuids.Contains(x.Uuid) == false) : source;
        }
    }
}
