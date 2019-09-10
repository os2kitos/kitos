using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByOrganizationId<T> : IDomainQuery<T>
        where T : class, IHasOrganization
    {
        private readonly int _organizationId;

        public QueryByOrganizationId(int organizationId)
        {
            _organizationId = organizationId;
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.OrganizationId == _organizationId);
        }
    }
}
