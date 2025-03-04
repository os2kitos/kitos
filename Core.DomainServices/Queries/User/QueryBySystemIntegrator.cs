using System.Linq;

namespace Core.DomainServices.Queries.User
{
    public class QueryBySystemIntegrator : IDomainQuery<DomainModel.User>
    {
        public IQueryable<DomainModel.User> Apply(IQueryable<DomainModel.User> source)
        {
            return source.Where(user => user.IsSystemIntegrator);
        }
    }
}
