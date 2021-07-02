using System.Collections.Generic;
using System.Linq;

namespace Core.DomainServices.Queries
{
    /// <summary>
    /// This query represents the intersection of the provided sub queries-
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntersectionQuery<T> : IDomainQuery<T>
    {
        private readonly IReadOnlyList<IDomainQuery<T>> _subQueries;

        public IntersectionQuery(IEnumerable<IDomainQuery<T>> subQueries)
        {
            _subQueries = subQueries.ToList().AsReadOnly();
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return _subQueries.Any() ? _subQueries.Aggregate(source, (state, next) => next.Apply(state)) : source;
        }
    }
}
