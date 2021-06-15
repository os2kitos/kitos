using System;
using System.Linq;

namespace Core.DomainServices.Queries.ItSystem
{
    public class QueryByRightsHolder : IDomainQuery<DomainModel.ItSystem.ItSystem>
    {
        private readonly Guid _rightsHolderUuid;

        public QueryByRightsHolder(Guid rightsHolderUuid)
        {
            _rightsHolderUuid = rightsHolderUuid;
        }

        public IQueryable<DomainModel.ItSystem.ItSystem> Apply(IQueryable<DomainModel.ItSystem.ItSystem> itSystems)
        {
            return itSystems.Where(x => x.BelongsTo != null && x.BelongsTo.Uuid == _rightsHolderUuid);
        }
    }
}
