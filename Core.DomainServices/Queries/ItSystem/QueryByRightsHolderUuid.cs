using System;
using System.Linq;

namespace Core.DomainServices.Queries.ItSystem
{
    public class QueryByRightsHolderUuid : IDomainQuery<DomainModel.ItSystem.ItSystem>
    {
        private readonly Guid _rightsHolderUuid;

        public QueryByRightsHolderUuid(Guid rightsHolderUuid)
        {
            _rightsHolderUuid = rightsHolderUuid;
        }

        public IQueryable<DomainModel.ItSystem.ItSystem> Apply(IQueryable<DomainModel.ItSystem.ItSystem> itSystems)
        {
            return itSystems.Where(x => x.BelongsTo != null && x.BelongsTo.Uuid == _rightsHolderUuid);
        }
    }
}
