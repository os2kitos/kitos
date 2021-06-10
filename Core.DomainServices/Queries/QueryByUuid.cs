using System;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByUuid<T> : IDomainQuery<T> where T : IHasUuid
    {
        private readonly Guid _uuid;

        public QueryByUuid(Guid uuid)
        {
            _uuid = uuid;
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.Uuid == _uuid);
        }
    }
}
