using System;
using System.Linq;

namespace Core.DomainServices.Queries.ItSystem
{
    public class QueryByBusinessType : IDomainQuery<DomainModel.ItSystem.ItSystem>
    {
        private readonly Guid _businessTypeId;

        public QueryByBusinessType(Guid businessTypeId)
        {
            _businessTypeId = businessTypeId;
        }

        public IQueryable<DomainModel.ItSystem.ItSystem> Apply(IQueryable<DomainModel.ItSystem.ItSystem> itSystems)
        {
            return itSystems.Where(x => x.BusinessType != null && x.BusinessType.Uuid == _businessTypeId);
        }
    }
}
