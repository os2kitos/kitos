using System.Linq;

namespace Core.DomainServices.Queries.Organization
{
    public class QueryByNameOrCvrContent : IDomainQuery<DomainModel.Organization.Organization>
    {
        private readonly string _query;

        public QueryByNameOrCvrContent(string query)
        {
            _query = query;
        }

        public IQueryable<DomainModel.Organization.Organization> Apply(IQueryable<DomainModel.Organization.Organization> source)
        {
            return source.Where(x => x.Cvr != null && x.Cvr.Contains(_query) || x.Name.Contains(_query));
        }
    }
}


