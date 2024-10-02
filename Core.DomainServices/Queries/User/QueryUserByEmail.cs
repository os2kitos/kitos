using System;
using System.Linq;

namespace Core.DomainServices.Queries.User
{
    public  class QueryUserByEmail : IDomainQuery<DomainModel.User>
    {
        private readonly string _query;

        public QueryUserByEmail(string query)
        {
            //Trim to remove leading or trailing whitespace ensuring that whitespace entered between first and last name does not result in no hit
            _query = query?.Trim() ?? throw new ArgumentNullException(nameof(query));
        }

        public IQueryable<DomainModel.User> Apply(IQueryable<DomainModel.User> source)
        {
            return _query
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(source, Match);
        }

        private static IQueryable<DomainModel.User> Match(IQueryable<DomainModel.User> original, string segment)
        {
            return original.Where(x => x.Email.Contains(segment));
        }
    }
}
