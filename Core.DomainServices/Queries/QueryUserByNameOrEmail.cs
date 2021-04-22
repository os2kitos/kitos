using System;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    /// <summary>
    /// Query to search users. To search a user the input string is split into segments by the ' ' char. Each segment must be present in either firstname, lastname or email
    /// </summary>
    public class QueryUserByNameOrEmail : IDomainQuery<User>
    {
        private readonly string _query;

        public QueryUserByNameOrEmail(string query)
        {
            //Trim to remove leading or trailing whitespace ensuring that whitespace entered between first and last name does not result in no hit
            _query = query?.Trim() ?? throw new ArgumentNullException(nameof(query));
        }

        public IQueryable<User> Apply(IQueryable<User> source)
        {
            return _query
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(source, Match);
        }

        private static IQueryable<User> Match(IQueryable<User> original, string segment)
        {
            return original.Where(x => x.Name.Contains(segment) || x.LastName.Contains(segment) || x.Email.Contains(segment));
        }
    }
}
