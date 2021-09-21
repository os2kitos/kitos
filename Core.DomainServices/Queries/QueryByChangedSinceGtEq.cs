using System;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByChangedSinceGtEq<T> : IDomainQuery<T>
        where T : class, IEntity
    {
        private readonly DateTime _since;

        public QueryByChangedSinceGtEq(DateTime since)
        {
            _since = since;
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.LastChanged >= _since);
        }
    }
}
