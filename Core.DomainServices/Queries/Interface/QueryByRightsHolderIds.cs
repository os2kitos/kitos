using Core.DomainModel.ItSystem;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainServices.Queries.Interface
{
    public class QueryByRightsHolderIds : IDomainQuery<ItInterface>
    {
        private readonly IEnumerable<int> _rightsHolderIds;

        public QueryByRightsHolderIds(IEnumerable<int> rightsHolderIds)
        {
            _rightsHolderIds = rightsHolderIds;
        }

        public IQueryable<ItInterface> Apply(IQueryable<ItInterface> itInterface)
        {
            return itInterface.Where(x => x.ExhibitedBy != null && x.ExhibitedBy.ItSystem != null && x.ExhibitedBy.ItSystem.BelongsToId != null && _rightsHolderIds.Contains(x.ExhibitedBy.ItSystem.BelongsToId.Value));
        }
    }
}
