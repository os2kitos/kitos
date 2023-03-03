using Core.DomainModel.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainServices.Queries.Notifications
{
    public class QueryByOwnerResource : IDomainQuery<DomainModel.Advice.Advice>
    {
        private readonly RelatedEntityType _ownerResourceType;

        public QueryByOwnerResource(RelatedEntityType ownerResourceType)
        {
            _ownerResourceType = ownerResourceType;
        }

        public IQueryable<DomainModel.Advice.Advice> Apply(IQueryable<DomainModel.Advice.Advice> source)
        {
            return source.Where(notification => notification.Type == _ownerResourceType);
        }
    }
}
