using System;
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Queries.KLE
{
    public class QueryByKeyPrefix : IDomainQuery<TaskRef>
    {
        private readonly string _prefix;
        private readonly string _prefixLower;

        public QueryByKeyPrefix(string prefix)
        {
            _prefix = (prefix ?? throw new ArgumentNullException(nameof(prefix))).Trim();
            _prefixLower = _prefix.ToLower();
        }

        public IQueryable<TaskRef> Apply(IQueryable<TaskRef> source)
        {
            return source.Where(x => x.TaskKey.ToLower().StartsWith(_prefixLower));
        }
    }
}
