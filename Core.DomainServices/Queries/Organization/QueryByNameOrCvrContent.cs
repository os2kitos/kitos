using System;
using System.Linq;

namespace Core.DomainServices.Queries.Organization
{
    public class QueryByNameOrCvrContent : IDomainQuery<DomainModel.Organization.Organization>
    {
        private readonly string? _queryLower;

        public QueryByNameOrCvrContent(string? query)
        {
            _queryLower = query?.ToLower();
        }

        public IQueryable<DomainModel.Organization.Organization> Apply(IQueryable<DomainModel.Organization.Organization> source)
        {
            if(string.IsNullOrEmpty(_queryLower))
                return source;

            return source.Where(x => x.Cvr != null && x.Cvr.ToLower().Contains(_queryLower) || x.Name.ToLower().Contains(_queryLower));
        }
    }
}
