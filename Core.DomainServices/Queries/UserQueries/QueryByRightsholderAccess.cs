using Core.DomainModel;
using Core.DomainModel.Organization;
using System.Linq;

namespace Core.DomainServices.Queries.UserQueries
{
    public class QueryByRightsholderAccess : IDomainQuery<User>
    {
        public IQueryable<User> Apply(IQueryable<User> users)
        {
            return users.Where(x => x.OrganizationRights.Any(x => x.Role == OrganizationRole.RightsHolderAccess));
        }
    }
}
