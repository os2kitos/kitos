using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryExceptEntitiesWithIds<T> : IDomainQuery<T>
    where T: Entity
    {
        private readonly IReadOnlyList<int> _ids;

        public QueryExceptEntitiesWithIds(IEnumerable<int> ids)
        {
            _ids = ids.ToList().AsReadOnly();
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => _ids.Contains(x.Id) == false);
        }
    }
}
