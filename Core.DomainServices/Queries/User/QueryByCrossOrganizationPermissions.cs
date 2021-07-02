using System.Linq;

namespace Core.DomainServices.Queries.User
{
    public class QueryByCrossOrganizationPermissions : IDomainQuery<DomainModel.User>
    {
        private readonly bool _includeStakeHolderAccess;
        private readonly bool _includeApiAccess;

        public QueryByCrossOrganizationPermissions(bool includeStakeHolderAccess = true, bool includeApiAccess = true)
        {
            _includeStakeHolderAccess = includeStakeHolderAccess;
            _includeApiAccess = includeApiAccess;
        }

        public IQueryable<DomainModel.User> Apply(IQueryable<DomainModel.User> users)
        {
            return users.Where(x => (_includeStakeHolderAccess && x.HasStakeHolderAccess)|| (_includeApiAccess && x.HasApiAccess == true));
        }
    }
}
