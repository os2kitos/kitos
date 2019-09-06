using System.Linq;

namespace Core.DomainServices.Queries
{
    public class RejectAllResultsQuery<T> : IDomainQuery<T>
    {
        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => false);
        }
    }
}
