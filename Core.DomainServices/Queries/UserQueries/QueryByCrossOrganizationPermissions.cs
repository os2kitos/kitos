using Core.DomainModel;
using System.Linq;

namespace Core.DomainServices.Queries.UserQueries
{
    public class QueryByCrossOrganizationPermissions : IDomainQuery<User>
    {
        private readonly bool _includeStakeHolderAccess;
        private readonly bool _includeApiAccess;

        public QueryByCrossOrganizationPermissions(bool includeStakeHolderAccess = true, bool includeApiAccess = true)
        {
            _includeStakeHolderAccess = includeStakeHolderAccess;
            _includeApiAccess = includeApiAccess;
        }

        public IQueryable<User> Apply(IQueryable<User> users)
        {
            return users.Where(x => x.HasStakeHolderAccess == _includeStakeHolderAccess || x.HasApiAccess == _includeApiAccess);
        }
    }
}
