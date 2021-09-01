using System;
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Queries.KLE
{
    public class QueryByKeyPrefix : IDomainQuery<TaskRef>
    {
        private readonly string _prefix;

        public QueryByKeyPrefix(string prefix)
        {
            _prefix = (prefix ?? throw new ArgumentNullException(nameof(prefix))).Trim();
        }

        public IQueryable<TaskRef> Apply(IQueryable<TaskRef> source)
        {
            return source.Where(x => x.TaskKey.StartsWith(_prefix));
        }
    }
}
