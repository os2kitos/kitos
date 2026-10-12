using System;
using System.Linq;

namespace Core.DomainServices.Queries.Organization
{
    public class QueryByCvrContent : IDomainQuery<DomainModel.Organization.Organization>
    {
        private readonly string? _cvrNumberContentLower;

        public QueryByCvrContent(string? cvrNumberContent)
        {
            _cvrNumberContentLower = cvrNumberContent?.ToLower();
        }

        public IQueryable<DomainModel.Organization.Organization> Apply(IQueryable<DomainModel.Organization.Organization> source)
        {
            if(string.IsNullOrEmpty(_cvrNumberContentLower))
                return source;

            return source.Where(x => x.Cvr != null && x.Cvr.ToLower().Contains(_cvrNumberContentLower));
        }
    }
}
