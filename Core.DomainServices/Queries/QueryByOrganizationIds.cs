using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByOrganizationIds<T> : IDomainQuery<T>
        where T : class, IOwnedByOrganization
    {
        private readonly int[] _organizationIds;

        public QueryByOrganizationIds(IEnumerable<int> organizationIds)
        {
            _organizationIds = organizationIds.ToArray();
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => _organizationIds.Contains(x.OrganizationId));
        }
    }
}
