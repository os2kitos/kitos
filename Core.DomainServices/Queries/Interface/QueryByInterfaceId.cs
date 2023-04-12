using System.Linq;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices.Queries.Interface
{
    public class QueryByInterfaceId : IDomainQuery<ItInterface>
    {
        private readonly string _id;

        public QueryByInterfaceId(string id)
        {
            _id = id;
        }

        public IQueryable<ItInterface> Apply(IQueryable<ItInterface> source)
        {
            return source.Where(x => x.ItInterfaceId == _id);
        }
    }
}
