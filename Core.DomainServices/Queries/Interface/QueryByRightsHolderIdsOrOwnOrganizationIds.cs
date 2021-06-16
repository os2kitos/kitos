using Core.DomainModel.ItSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainServices.Queries.Interface
{
    public class QueryByRightsHolderIdsOrOwnOrganizationIds : IDomainQuery<ItInterface>
    {
        private readonly IEnumerable<int> _rightsHolderIds;
        private readonly IEnumerable<int> _ownOrganizationIds;

        public QueryByRightsHolderIdsOrOwnOrganizationIds(IEnumerable<int> rightsHolderIds = null, IEnumerable<int> ownOrganizationIds = null)
        {
            _rightsHolderIds = rightsHolderIds?.ToList() ?? new List<int>();
            _ownOrganizationIds = ownOrganizationIds?.ToList() ?? new List<int>();

            if (!_rightsHolderIds.Any() && !_ownOrganizationIds.Any())
                throw new ArgumentException("No constraints provided - query will reject all input");
        }

        public IQueryable<ItInterface> Apply(IQueryable<ItInterface> itInterface)
        {
            return itInterface.Where(x =>
            _ownOrganizationIds.Contains(x.OrganizationId) ||
            (x.ExhibitedBy != null &&
                x.ExhibitedBy.ItSystem != null &&
                x.ExhibitedBy.ItSystem.BelongsToId != null &&
                _rightsHolderIds.Contains(x.ExhibitedBy.ItSystem.BelongsToId.Value)));
        }
    }
}
