using System.Linq;

namespace Core.DomainServices.Queries.User
{
    public class QueryByGlobalAdmin : IDomainQuery<DomainModel.User>
    {

        public IQueryable<DomainModel.User> Apply(IQueryable<DomainModel.User> users)
        {
            return users.Where(user => user.IsGlobalAdmin);
        }
    }
}
