using Core.DomainModel.GDPR;
using System;
using System.Linq;

namespace Core.DomainServices.Queries.DPR
{
    public class QueryBySystemUsageUuid : IDomainQuery<DataProcessingRegistration>
    {
        private readonly Guid _usageId;

        public QueryBySystemUsageUuid(Guid usageId)
        {
            _usageId = usageId;
        }

        public IQueryable<DataProcessingRegistration> Apply(IQueryable<DataProcessingRegistration> source)
        {
            return source.Where(dataProcessingRegistration => dataProcessingRegistration.SystemUsages.Any(usage => usage.Uuid == _usageId));
        }
    }
}
