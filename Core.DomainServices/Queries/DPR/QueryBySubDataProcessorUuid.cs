using Core.DomainModel.GDPR;
using System;
using System.Linq;

namespace Core.DomainServices.Queries.DPR
{
    public class QueryBySubDataProcessorUuid : IDomainQuery<DataProcessingRegistration>
    {
        private readonly Guid _subDataProcessorId;

        public QueryBySubDataProcessorUuid(Guid subDataProcessorId)
        {
            _subDataProcessorId = subDataProcessorId;
        }

        public IQueryable<DataProcessingRegistration> Apply(IQueryable<DataProcessingRegistration> source)
        {
            return source.Where(dataProcessingRegistration => dataProcessingRegistration.SubDataProcessors.Any(subDataProcessor => subDataProcessor.Uuid == _subDataProcessorId));
        }
    }
}
