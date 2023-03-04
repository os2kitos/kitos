using Core.DomainModel.Shared;
using System.Linq;

namespace Core.DomainServices.Queries.Notifications
{
    public class QueryByOwnerResourceType : IDomainQuery<DomainModel.Advice.Advice>
    {
        private readonly RelatedEntityType _ownerResourceType;

        public QueryByOwnerResourceType(RelatedEntityType ownerResourceType)
        {
            _ownerResourceType = ownerResourceType;
        }

        public IQueryable<DomainModel.Advice.Advice> Apply(IQueryable<DomainModel.Advice.Advice> source)
        {
            return source.Where(notification => notification.Type == _ownerResourceType);
        }
    }
}
