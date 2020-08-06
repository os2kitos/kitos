using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByPublicAccessOrOrganizationIds<T> : IDomainQuery<T>
    where T : class, IHasAccessModifier, IOwnedByOrganization
    {
        private readonly int[] _organizationIds;

        public QueryByPublicAccessOrOrganizationIds(IEnumerable<int> organizationIds)
        {
            _organizationIds = organizationIds.ToArray();
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.AccessModifier == AccessModifier.Public || _organizationIds.Contains(x.OrganizationId));
        }
    }
}
