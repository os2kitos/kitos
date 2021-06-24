using Core.DomainModel;
using System.Linq;

namespace Core.DomainServices.Queries
{
    public class QueryByEnabledEntitiesOnly<T> : IDomainQuery<T> where T : class, IEntityWithEnabledStatus
    {
        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.Disabled == false);
        }
    }
}
