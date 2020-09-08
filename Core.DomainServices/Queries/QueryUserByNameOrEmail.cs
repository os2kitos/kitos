using System;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryUserByNameOrEmail : IDomainQuery<User>
    {
        private readonly string _query;

        public QueryUserByNameOrEmail(string query)
        {
            _query = query ?? throw new ArgumentNullException(nameof(query));
        }

        public IQueryable<User> Apply(IQueryable<User> source)
        {
            return source.Where(x => x.Name.Contains(_query) || x.Email.Contains(_query));
        }
    }
}
