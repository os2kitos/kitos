using System.Linq;

namespace Core.DomainServices.Queries.Organization
{
    public class QueryByNameOrCvrContent : IDomainQuery<DomainModel.Organization.Organization>
    {
        private readonly string _searchContent;

        public QueryByNameOrCvrContent(string searchContent)
        {
            _searchContent = searchContent;
        }

        public IQueryable<DomainModel.Organization.Organization> Apply(IQueryable<DomainModel.Organization.Organization> source)
        {
            return source.Where(x => !string.IsNullOrEmpty(x.Cvr) && x.Cvr.Contains(_searchContent) || x.Name.Contains(_searchContent));
        }
    }
}


