using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainServices.Queries.ItSystem
{
    public class QueryByRightsHolderIds : IDomainQuery<DomainModel.ItSystem.ItSystem>
    {
        private readonly IEnumerable<int> _organizationIds;

        public QueryByRightsHolderIds(IEnumerable<int> organizationIds)
        {
            _organizationIds = organizationIds?.ToList() ?? throw new ArgumentNullException(nameof(organizationIds));
        }

        public IQueryable<DomainModel.ItSystem.ItSystem> Apply(IQueryable<DomainModel.ItSystem.ItSystem> source)
        {
            return source.Where(x => x.BelongsToId != null && _organizationIds.Contains(x.BelongsToId.Value));
        }
    }
}
