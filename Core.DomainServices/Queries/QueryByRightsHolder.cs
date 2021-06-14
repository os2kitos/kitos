using Core.DomainModel.ItSystem;
using System;
using System.Linq;

namespace Core.DomainServices.Queries.ItSystem
{
    public class QueryByRightsHolder : IDomainQuery<ItInterface>
    {
        private readonly Guid _rightsHolderUuid;

        public QueryByRightsHolder(Guid rightsHolderUuid)
        {
            _rightsHolderUuid = rightsHolderUuid;
        }

        public IQueryable<ItInterface> Apply(IQueryable<ItInterface> itInterface)
        {
            return itInterface.Where(x => x.ExhibitedBy.ItSystem.BelongsTo != null && x.ExhibitedBy.ItSystem.BelongsTo.Uuid == _rightsHolderUuid);
        }
    }
}