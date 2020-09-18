using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByIds<T> : IDomainQuery<T>
        where T : class,IHasId
    {
        private readonly IReadOnlyList<int> _ids;

        public QueryByIds(IEnumerable<int> ids)
        {
            _ids = ids.ToList().AsReadOnly();
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            //If no ids provided, remove all results. If not filter by ids
            return _ids.Any() ? source.Where(x => _ids.Contains(x.Id)) : new RejectAllResultsQuery<T>().Apply(source);
        }
    }
}
