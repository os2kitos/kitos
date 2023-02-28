using System.Linq;
using Core.DomainModel.Shared;

namespace Core.DomainServices.Queries.Notifications
{
    public class QueryByOwnerResourceId : IDomainQuery<DomainModel.Advice.Advice>
    {
        private readonly int _ownerResourceId;
        private readonly RelatedEntityType _ownerResourceType;

        public QueryByOwnerResourceId(int ownerResourceId, RelatedEntityType ownerResourceType)
        {
            _ownerResourceId = ownerResourceId;
            _ownerResourceType = ownerResourceType;
        }

        public IQueryable<DomainModel.Advice.Advice> Apply(IQueryable<DomainModel.Advice.Advice> source)
        {
            return source.Where(notification => notification.RelationId == _ownerResourceId && notification.Type == _ownerResourceType);
        }
    }
}
