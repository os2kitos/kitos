using System;
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Queries.KLE
{
    public class QueryByParentKey : IDomainQuery<TaskRef>
    {
        private readonly string _parentKey;

        public QueryByParentKey(string parentKey)
        {
            _parentKey = (parentKey ?? throw new ArgumentNullException(nameof(parentKey))).Trim();
        }

        public IQueryable<TaskRef> Apply(IQueryable<TaskRef> source)
        {
            return source.Where(x => x.Parent != null && x.Parent.TaskKey == _parentKey);
        }
    }
}
