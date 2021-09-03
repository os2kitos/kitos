using Core.DomainModel.GDPR;
using System;
using System.Linq;

namespace Core.DomainServices.Queries.DPR
{
    public class QueryByDataProcessorUuid : IDomainQuery<DataProcessingRegistration>
    {
        private readonly Guid _dataProcessorId;

        public QueryByDataProcessorUuid(Guid dataProcessorId)
        {
            _dataProcessorId = dataProcessorId;
        }

        public IQueryable<DataProcessingRegistration> Apply(IQueryable<DataProcessingRegistration> source)
        {
            return source.Where(dataProcessingRegistration => dataProcessingRegistration.DataProcessors.Any(dataProcessor => dataProcessor.Uuid == _dataProcessorId));
        }
    }
}
