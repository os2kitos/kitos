using System.Linq;
using Core.DomainModel.Shared;

namespace Core.DomainServices.Queries.Notifications
{
    public class QueryByOwnerResourceId : IDomainQuery<DomainModel.Advice.Advice>
    {
        private readonly int _ownerResourceId;

        public QueryByOwnerResourceId(int ownerResourceId)
        {
            _ownerResourceId = ownerResourceId;
        }

        public IQueryable<DomainModel.Advice.Advice> Apply(IQueryable<DomainModel.Advice.Advice> source)
        {
            return source.Where(notification => notification.RelationId == _ownerResourceId);
        }
    }
}
