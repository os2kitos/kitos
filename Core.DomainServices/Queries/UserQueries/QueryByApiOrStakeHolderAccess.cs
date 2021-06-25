using Core.DomainModel;
using System.Linq;

namespace Core.DomainServices.Queries.UserQueries
{
    public class QueryByApiOrStakeHolderAccess : IDomainQuery<User>
    {
        public IQueryable<User> Apply(IQueryable<User> users)
        {
            return users.Where(x => x.HasStakeHolderAccess || (x.HasApiAccess.HasValue && x.HasApiAccess.Value));
        }
    }
}
