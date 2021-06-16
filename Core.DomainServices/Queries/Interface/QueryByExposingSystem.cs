using Core.DomainModel.ItSystem;
using System;
using System.Linq;

namespace Core.DomainServices.Queries.Interface
{
    public class QueryByExposingSystem : IDomainQuery<ItInterface>
    {
        private readonly Guid _exposingSystemUuid;

        public QueryByExposingSystem(Guid exposingSystemUuid)
        {
            _exposingSystemUuid = exposingSystemUuid;
        }

        public IQueryable<ItInterface> Apply(IQueryable<ItInterface> itInterface)
        {
            return itInterface.Where(x => x.ExhibitedBy != null && x.ExhibitedBy.ItSystem != null && x.ExhibitedBy.ItSystem.Uuid == _exposingSystemUuid);
        }
    }
}
