using Core.DomainModel.GDPR;
using System;
using System.Linq;

namespace Core.DomainServices.Queries.DPR
{
    public class QueryBySystemUuid : IDomainQuery<DataProcessingRegistration>
    {
        private readonly Guid _systemUuid;

        public QueryBySystemUuid(Guid systemUuid)
        {
            _systemUuid = systemUuid;
        }

        public IQueryable<DataProcessingRegistration> Apply(IQueryable<DataProcessingRegistration> source)
        {
            return source.Where(dataProcessingRegistration => dataProcessingRegistration.SystemUsages.Any(systemUsage => systemUsage.ItSystem.Uuid == _systemUuid));
        }
    }
}
