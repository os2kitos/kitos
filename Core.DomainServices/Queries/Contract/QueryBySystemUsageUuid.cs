using System;
using System.Linq;
using Core.DomainModel.ItContract;

namespace Core.DomainServices.Queries.Contract
{
    public class QueryBySystemUsageUuid : IDomainQuery<ItContract>
    {
        private readonly Guid _usageId;

        public QueryBySystemUsageUuid(Guid usageId)
        {
            _usageId = usageId;
        }

        public IQueryable<ItContract> Apply(IQueryable<ItContract> source)
        {
            return source.Where(contract => contract.AssociatedSystemUsages.Any(usage => usage.ItSystemUsage.Uuid == _usageId));
        }
    }
}
