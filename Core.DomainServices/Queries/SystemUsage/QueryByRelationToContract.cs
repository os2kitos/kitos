using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices.Queries.SystemUsage
{
    public class QueryByRelationToContract : IDomainQuery<ItSystemUsage>
    { 
        private readonly Guid _contractUuid;

        public QueryByRelationToContract(Guid contractUuid)
        {
            _contractUuid = contractUuid;
        }

        public IQueryable<ItSystemUsage> Apply(IQueryable<ItSystemUsage> source)
        {
            return source.Where(systemUsage => systemUsage.UsageRelations.Any(relation => relation.AssociatedContract != null && relation.AssociatedContract.Uuid == _contractUuid));
        }
    }
}
