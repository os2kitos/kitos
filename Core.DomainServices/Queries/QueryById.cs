using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryById<T> : IDomainQuery<T>
        where T : Entity
    {
        private readonly int _id;

        public QueryById(int id)
        {
            _id = id;
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.Id == _id);
        }
    }
}
