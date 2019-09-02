using System.Linq;

namespace Core.DomainServices.Queries
{
    public interface IDomainQuery<T>
    {
        IQueryable<T> Apply(IQueryable<T> source);
    }
}
