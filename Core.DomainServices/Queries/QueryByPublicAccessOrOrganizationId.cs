using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByPublicAccessOrOrganizationId<T> : IDomainQuery<T>
    where T : class, IHasAccessModifier, IOwnedByOrganization
    {
        private readonly int _organizationId;

        public QueryByPublicAccessOrOrganizationId(int organizationId)
        {
            _organizationId = organizationId;
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.AccessModifier == AccessModifier.Public || x.OrganizationId == _organizationId);
        }
    }
}
