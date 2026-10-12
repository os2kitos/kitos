using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainServices.Queries.ItSystem
{
    public class QueryByRightsHolderIdOrOwnOrganizationIds : IDomainQuery<DomainModel.ItSystem.ItSystem>
    {
        private readonly IEnumerable<int> _ownOrganizationIds;
        private readonly IEnumerable<int> _rightsHoldersOrganizationIds;

        public QueryByRightsHolderIdOrOwnOrganizationIds(IEnumerable<int>? organizationIds = null, IEnumerable<int>? ownOrganizationIds = null)
        {
            _ownOrganizationIds = ownOrganizationIds?.ToList() ?? [];
            _rightsHoldersOrganizationIds = organizationIds?.ToList() ?? [];

            if (!_ownOrganizationIds.Any() && !_rightsHoldersOrganizationIds.Any())
                throw new ArgumentException("No constraints provided - query will reject all input");
        }

        public IQueryable<DomainModel.ItSystem.ItSystem> Apply(IQueryable<DomainModel.ItSystem.ItSystem> source)
        {
            return source.Where(itSystem => _ownOrganizationIds.Contains(itSystem.OrganizationId) || (itSystem.BelongsToId != null && _rightsHoldersOrganizationIds.Contains(itSystem.BelongsToId.Value)));
        }
    }
}
