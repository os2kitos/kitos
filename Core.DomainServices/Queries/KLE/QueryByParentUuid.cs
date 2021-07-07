using System;
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Queries.KLE
{
    public class QueryByParentUuid : IDomainQuery<TaskRef>
    {
        private readonly Guid _parentUuid;

        public QueryByParentUuid(Guid parentUuid)
        {
            _parentUuid = parentUuid;
        }

        public IQueryable<TaskRef> Apply(IQueryable<TaskRef> source)
        {
            return source.Where(x => x.Parent != null && x.Parent.Uuid == _parentUuid);
        }
    }
}
