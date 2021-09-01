using System.Linq;

namespace Core.DomainServices.Queries.Organization
{
    public class QueryByCvrContent : IDomainQuery<DomainModel.Organization.Organization>
    {
        private readonly string _cvrNumberContent;

        public QueryByCvrContent(string cvrNumberContent)
        {
            _cvrNumberContent = cvrNumberContent;
        }

        public IQueryable<DomainModel.Organization.Organization> Apply(IQueryable<DomainModel.Organization.Organization> source)
        {
            return source.Where(x => x.Cvr != null && x.Cvr.Contains(_cvrNumberContent));
        }
    }
}
