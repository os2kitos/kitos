using System.Linq;
using System;

namespace Core.DomainServices.Queries.ItSystem
{
    public class QueryExceptChildrenOfUuid : IDomainQuery<DomainModel.ItSystem.ItSystem>
    {
        private readonly Guid _uuid;

        public QueryExceptChildrenOfUuid(Guid uuid)
        {
            _uuid = uuid;
        }

        public IQueryable<DomainModel.ItSystem.ItSystem> Apply(IQueryable<DomainModel.ItSystem.ItSystem> source)
        {
            //If not ids, return source query. If any return the filtered query
            return source.Where(x => x.Parent == null || x.Parent.Uuid != _uuid);
        }
    }
}
