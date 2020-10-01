using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryExceptEntitiesWithIds<T> : IDomainQuery<T>
    where T: class, IHasId
    {
        private readonly IReadOnlyList<int> _ids;

        public QueryExceptEntitiesWithIds(IEnumerable<int> ids)
        {
            _ids = ids.ToList().AsReadOnly();
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            //If not ids, return source query. If any return the filtered query
            return _ids.Any() ? source.Where(x => _ids.Contains(x.Id) == false) : source;
        }
    }
}
