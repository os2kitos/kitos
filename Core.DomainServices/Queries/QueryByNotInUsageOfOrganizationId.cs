using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByNotInUsageOfOrganizationId<T> : IDomainQuery<T>
    where T : class, IHasUsages
    {

        private readonly int _organizationId;

        public QueryByNotInUsageOfOrganizationId(int organizationId)
        {
            _organizationId = organizationId;
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.Usages.Count(y => y.OrganizationId == _organizationId) == 0);
        }
    }
}
