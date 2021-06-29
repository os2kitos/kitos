using Core.DomainModel;
using Core.DomainModel.Organization;
using System.Linq;

namespace Core.DomainServices.Queries.UserQueries
{
    public class QueryByRoleAssignment : IDomainQuery<User>
    {
        private readonly OrganizationRole _role;

        public QueryByRoleAssignment(OrganizationRole role)
        {
            _role = role;
        }

        public IQueryable<User> Apply(IQueryable<User> users)
        {
            return users.Where(x => x.OrganizationRights.Any(x => x.Role == _role));
        }
    }
}
