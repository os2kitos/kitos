using System.Linq;

namespace Core.DomainServices.Queries.Organization
{
    public class QueryByIsSupplier : IDomainQuery<DomainModel.Organization.Organization>
    {
        private readonly bool _query;

        public QueryByIsSupplier(bool query)
        {
            _query = query;
        }

        public IQueryable<DomainModel.Organization.Organization> Apply(IQueryable<DomainModel.Organization.Organization> source)
        {
            return source.Where(x => x.IsSupplier == _query);
        }
    }
}
